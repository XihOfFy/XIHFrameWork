using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace XiHNet
{
    public class PBProxy
    {
        private readonly Dictionary<ushort, Action<byte[]>> ntfDefActs = new Dictionary<ushort, Action<byte[]>>();//自定义回调集合-通知
        private ushort tid = 0;
        private readonly Dictionary<ushort, TaskCompletionSource<IMessage>> tcsDics = new Dictionary<ushort, TaskCompletionSource<IMessage>>(64);//自定义回调集合-响应
        private static T ConvertBytes2PBMsg<T>(byte[] data) where T : IMessage
        {
            using MemoryStream stream = new MemoryStream(data);
            return ProtoBuf.Serializer.Deserialize<T>(stream);
        }
        /*private static object ConvertBytes2PBMsg(byte[] data,Type type)
        {
            using MemoryStream stream = new MemoryStream(data);
            return ProtoBuf.Serializer.Deserialize(type,stream);
        }*/
        public (byte[], TaskCompletionSource<IMessage>) SendReq(IMessage req)
        {
            req.TaskId = ++tid;
            using MemoryStream stream = new MemoryStream();
            ProtoBuf.Serializer.Serialize(stream, req);
            if (tcsDics.ContainsKey(req.TaskId))
            {
                tcsDics[req.TaskId].SetResult(NullMessage.IMessageNull);
                Debugger.Log($"<color=green>已有新的请求{req}【{req.TaskId}】，将使之前任务直接置空 </color>");
            }
            tcsDics[req.TaskId] = new TaskCompletionSource<IMessage>();
            //Debugger.Log($"<color=green>发送Req: </color>{req}");
            return (stream.ToArray(), tcsDics[req.TaskId]);
        }
        public Action DecodeRsp(byte[] data, ushort msgType)
        {
            if (data == null) return null;
            if (IMessageExt.IsRespone(msgType))
            {
                using MemoryStream stream = new MemoryStream(data);
                if (ProtoBuf.Serializer.Deserialize(IMessageExt.GetClassType(msgType), stream) is IMessage msg && tcsDics.ContainsKey(msg.TaskId))
                {
                    TaskCompletionSource<IMessage> tcs = tcsDics[msg.TaskId];
                    tcsDics.Remove(msg.TaskId);
                    return () => tcs.SetResult(msg);
                }
            }
            else
            {
                if (ntfDefActs.TryGetValue(msgType, out Action<byte[]> act))
                {
                    return () => act(data);
                }
            }
            Debugger.Log($"<color=red>未知: 无 {msgType} 对应的处理方法</color>");
            return null;
        }
        public void RegisterNtf<Ntf>(Action<Ntf> handler) where Ntf : IMessage
        {
            if (handler == null) return;
#if XIHSERVER
            ntfDefActs[IMessageExt.GetMsgType<Ntf>()] = (rsp) => { handler(ConvertBytes2PBMsg<Ntf>(rsp)); };
#else
            //ILRuntime对泛型支持不太行
            ushort tp = IMessageExt.GetMsgType<Ntf>();
            ntfDefActs[tp] = (rsp) => {
                using MemoryStream stream = new MemoryStream(rsp);
                if (ProtoBuf.Serializer.Deserialize(IMessageExt.GetClassType(tp), stream) is IMessage msg)
                {
                    handler((Ntf)msg);
                }
            };
#endif
        }
        /// <summary>
        /// 若是Rsp则必须与Req的TaskID相同,Ntf则可默认值
        /// </summary>
        /// <typeparam name="Ntf"></typeparam>
        /// <param name="msg"></param>
        /// <returns></returns>
        public byte[] SendNtf(IMessage msg)
        {
            //Debugger.Log($"<color=green>发送Ntf: </color>{msg}");
            using MemoryStream stream = new MemoryStream();
            ProtoBuf.Serializer.Serialize(stream, msg);
            //Debugger.Log($"SendNtf : {BitConverter.ToString(arr)}");
            return stream.ToArray();
        }
        public void Dispose()
        {
            //ntfDefActs.Clear();
            foreach (var tcs in tcsDics)
            {
                tcs.Value.SetResult(NullMessage.IMessageNull);
            }
            tcsDics.Clear();
        }
    }
}
