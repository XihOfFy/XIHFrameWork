using System;
using System.Net;

namespace XiHNet
{
    public class NetAdapter
    {
        public Action<byte[]> OnMessageAct { get; }
        public Action OnConnectedAct { get;}
        public Action OnClosedAct { get;}
        private readonly NetClient netClient;
        private readonly ICryptor cryptor;
        private bool closed;
        public NetAdapter(NetworkProtocol protocol, IPEndPoint iPEnd, CryptType cryptType,byte[] key=null)
        {
            switch (protocol)
            {
                case NetworkProtocol.Kcp:
                    netClient = new KcpClient(iPEnd);
                    break;
                case NetworkProtocol.Tcp:
                    netClient = new TcpClient(iPEnd);
                    break;
                case NetworkProtocol.WXTCP:
                    break;
                case NetworkProtocol.WXUDP:
                    break;
                default:
                    //TODO 自行添加
                    break;
            }
            cryptor = cryptType switch
            {
                CryptType.CryptAes => new AesCryptor(key),
                CryptType.CryptXor => new XorCryptor(key),
                _ => NoneCryptor.Default,
            };
            netClient.OnConnectedAct = OnConnectedAct;
            netClient.OnMessageAct = OnMessage;
            netClient.OnClosedAct = Close;
            closed = true;
        }
        public void Connect()
        {
            if (!closed) return;
            netClient.Connect();
        }
        public void Close()
        {
            if (closed) return;
            closed = true;
            netClient.Close();
            OnClosedAct?.Invoke();
        }
        public void Request(byte[] data)
        {
            if (closed) return;
            cryptor.Encrypt(data, 0, data.Length, out byte[] opt);
            netClient.Request(opt);
        }
        private void OnMessage(byte[] data)
        {
            (bool suc, byte[] rem, ushort msgType, byte[] body) = NetConfig.UnpackBody(data);
            if (suc)
            {
                cryptor.Decrypt(body, 0, body.Length, out byte[] opt);
                OnMessageAct?.Invoke(opt);
                if (rem != null)//防止粘包
                    OnMessage(rem);
            }
            else
            {
                OnClosedAct?.Invoke();
            }
        }
    }
}
