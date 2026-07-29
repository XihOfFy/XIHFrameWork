using System;
using System.Net;

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
        public abstract void Connect();
        public abstract void Request(byte[] data);
        /// <summary>
        /// 这个可能是异步线程回调，所以要考虑是否使用了主线程才能用的方法
        /// </summary>
        public Action<byte[]> OnMessageAct { get; set; }
        public Action OnConnectedAct { get; set; }
        public Action OnClosedAct { get; set; }
        public abstract void Close();
    }
}
