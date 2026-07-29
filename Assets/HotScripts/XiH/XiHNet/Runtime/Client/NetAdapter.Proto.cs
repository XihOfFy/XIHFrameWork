using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace XiHNet
{
    public partial class NetAdapter
    {
        public readonly PBProxy pbProxy;
        readonly ConcurrentQueue<Action> processQueue = new ConcurrentQueue<Action>();

        public Task<IMessage> SendReq<Req>(Req req) where Req : IMessage
        {
            var (data, tcs) = pbProxy.SendReq(req);
            Request(data, req.GetMsgType(), req.Route, req.TargetSession);
            return tcs.Task;
        }

        public void RegisterNtf<Ntf>(Action<Ntf> handler) where Ntf : IMessage
        {
            pbProxy.RegisterNtf(handler);
        }

        public void SendNtf<Ntf>(Ntf ntf) where Ntf : IMessage
        {
            var data = pbProxy.SendNtf(ntf);
            Request(data, ntf.GetMsgType(), ntf.Route, ntf.TargetSession);
        }

        void PBProxyOnMessage(byte[] data, ushort msgType, MsgRoute frameRoute, ulong frameTargetSession)
        {
            var waitProcessAct = pbProxy.DecodeRsp(data, msgType, frameRoute, frameTargetSession);
            if (waitProcessAct == null) return;
            processQueue.Enqueue(waitProcessAct);
        }

        public void UpdateMessageQueue()
        {
            while (processQueue.TryDequeue(out var act))
            {
                act?.Invoke();
            }
        }
    }
}
