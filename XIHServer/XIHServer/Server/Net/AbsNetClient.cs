using System;
using System.Collections.Generic;
using System.IO;
using XiHNet;

namespace XIHServer
{
    public abstract class AbsNetClient
    {
        protected bool isClosed = false;
        protected readonly Action onClosedAct;
        protected ICryptor cryptor;
        private readonly Dictionary<ushort, Action<AbsNetClient, byte[]>> handles;
        //public Queue<IMessage> WaitingSendQue { get; } = new Queue<IMessage>();
        public OnLineLobbyPlayer Player { get; set; }
        public ulong SessionKey { get; set; }
		public BattleMap Map { get; set; }
        public ClientAuth AuthStatus { get; set; }
        public AbsNetClient(Action onClosed, Dictionary<ushort, Action<AbsNetClient, byte[]>> handles) {
            onClosedAct = onClosed;
            this.handles = handles;
            AuthStatus = ClientAuth.None;
        }
        public abstract void Send<Msg>(Msg msg) where Msg : IMessage;
        protected byte[] Map2Bytes<Msg>(Msg msg) where Msg : IMessage
        {
            using MemoryStream stream = new MemoryStream();
            ProtoBuf.Serializer.Serialize(stream, msg);
            byte[] data = stream.ToArray();
            cryptor.Encrypt(data, 0, data.Length, out byte[] opt);
            return NetConfig.BuildData(opt, msg.GetMsgType());
        }
        protected void OnMsg(byte[] data) {
            if (isClosed) return;
            (bool suc, byte[] rem, ushort msgType, byte[] body) = NetConfig.UnpackBody(data);
            if (suc)
            {
                if (!CommonHandle.IgnoreAuth.Contains(msgType))
                {
                    if (AuthStatus != ClientAuth.Authed) {
                        Send(new KickOutNtf());//客户端应该移除连接并关闭，不再轮询
                        return;
                    }
                }
                cryptor.Decrypt(body, 0, body.Length, out byte[] opt);
                if (handles.ContainsKey(msgType))
                {
                    Program.ActQues.Enqueue(()=> { handles[msgType](this, opt); });
                }
                else
                {
                    Debugger.Log($"MsgType={msgType}对应的处理方式不存在，请检查是否请求了正确的服务端口");
                }
                if (rem != null)
                    OnMsg(rem);
            }
            else
            {
                Close();
            }
        }
        public abstract void Close();
    }
}
