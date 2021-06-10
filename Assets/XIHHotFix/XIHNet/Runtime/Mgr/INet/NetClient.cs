using System;
using System.Net;
using System.Threading.Tasks;

namespace XiHNet
{
    public abstract class NetClient
    {
        protected readonly IPEndPoint _endPoint;
        public NetClient(IPEndPoint iPEnd)
        {
            _endPoint = iPEnd;
        }
        public NetState NetState { get; protected set; } = NetState.Closed;
        public abstract Task<bool> ConnectAsync();
        public abstract Task<bool> RequestAsync(byte[] data);
        public Action<byte[]> OnMessage { get; set; }
        public Action OnClosed { get; set; }
        public abstract void Close();
    }
}
