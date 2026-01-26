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
        public ulong SessionKey { get; set; }
        public AbsNetClient(Action onClosed, Dictionary<ushort, Action<AbsNetClient, byte[]>> handles) {
            onClosedAct = onClosed;
            this.handles = handles;
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
            
        }
        public abstract void Close();
    }
}
