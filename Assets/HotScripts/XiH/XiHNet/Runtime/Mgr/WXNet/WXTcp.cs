#if UNITY_WX// && !UNITY_EDITOR
#define UNITY_WX_WITHOUT_EDITOR
#endif
#if UNITY_WX
using WeChatWASM;
#endif
using System.Net;
using UnityEngine;

namespace XiHNet
{
    public class WXTcp : NetClient
    {
#if UNITY_WX_WITHOUT_EDITOR
        WXTCPSocket socket;
        public WXTcp(IPEndPoint iPEnd) : base(iPEnd)
        {
            socket = WX.CreateTCPSocket();
            socket.OnError(OnError);
            socket.OnConnect(OnConnect);
            socket.OnMessage(OnMessage);
        }
        void OnError(GeneralCallbackResult result) {
            Close();
        }
        void OnConnect(GeneralCallbackResult result) {
            NetState = NetState.Open;
            OnConnectedAct?.Invoke();
        }
        void OnMessage(TCPSocketOnMessageListenerResult result) {
            OnMessageAct?.Invoke(result.message);
        }


        public override void Close()
        {
            if (NetState.Closed== NetState) return;
            NetState = NetState.Closed;
            OnClosedAct?.Invoke();
            socket.OffError(OnError);
            socket.OffConnect(OnConnect);
            socket.OffMessage(OnMessage);
            socket.Close();
        }
        public override void Connect()
        {
            if (NetState.Open == NetState) return;
            socket.Connect(new TCPSocketConnectOption() { 
                address= _endPoint.Address.ToString(), 
                port= _endPoint.Port
            });
        }
        public override void Request(byte[] data)
        {
            if (NetState.Closed == NetState) return;
            socket.Write(data);
        }
#else
        public WXTcp(IPEndPoint iPEnd) : base(iPEnd)
        {
            Debug.LogError("当前平台环境不支持微信小游戏TCP协议");
        }
        public override void Close()
        {
        }
        public override void Connect()
        {
        }
        public override void Request(byte[] data)
        {
        }
#endif
    }
}
