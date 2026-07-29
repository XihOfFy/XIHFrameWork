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
    public class WXUdp : NetClient
    {
#if UNITY_WX_WITHOUT_EDITOR
        WXUDPSocket socket;
        string ipAddr;
        public WXUdp(IPEndPoint iPEnd) : base(iPEnd)
        {
            ipAddr = iPEnd.Address.ToString();
            socket = WX.CreateUDPSocket();
            socket.OnError(OnError);
            socket.OnMessage(OnMessage);
        }
        void OnError(GeneralCallbackResult result)
        {
            Close();
        }
        void OnConnect()
        {
            NetState = NetState.Open;
            OnConnectedAct?.Invoke();
        }
        void OnMessage(UDPSocketOnMessageListenerResult result)
        {
            OnMessageAct?.Invoke(result.message);
        }

        public override void Close()
        {
            if (NetState.Closed == NetState) return;
            NetState = NetState.Closed;
            OnClosedAct?.Invoke();
            socket.OffError(OnError);
            socket.OffMessage(OnMessage);
            socket.Close();
        }
        public override void Connect()
        {
            if (NetState.Open == NetState) return;
            socket.Connect(new UDPSocketConnectOption()
            {
                address = ipAddr,
                port = _endPoint.Port
            });
            socket.Write(new UDPSocketSendOption() { 
                address = ipAddr,
                port = _endPoint.Port
            });
            OnConnect();
        }
        public override void Request(byte[] data)
        {
            if (NetState.Closed == NetState) return;
            socket.Send(new UDPSocketSendOption() { 
                address = ipAddr,
                port = _endPoint.Port,
                length = data.Length,
                message = data,
            });
        }
#else
        public WXUdp(IPEndPoint iPEnd) : base(iPEnd)
        {
            Debug.LogError("当前平台环境不支持微信小游戏UDP协议");
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
