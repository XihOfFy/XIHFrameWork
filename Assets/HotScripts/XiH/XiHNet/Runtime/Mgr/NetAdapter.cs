using System;
using System.IO;
using System.Net;
using UnityEngine;

namespace XiHNet
{
    public class NetAdapter
    {
        public Action<byte[]> OnMessageAct { get; set; }
        public Action OnConnectedAct { get; set; }
        public Action OnClosedAct { get; set; }
        private readonly NetClient netClient;
        private ICryptor cryptor;
        private bool closed;
        public NetAdapter(NetworkProtocol protocol, IPEndPoint iPEnd)
        {
            switch (protocol)
            {
#if !UNITY_WEBGL || UNITY_EDITOR
                case NetworkProtocol.Kcp:
                    netClient = new KcpClient(iPEnd);
                    break;
                case NetworkProtocol.Tcp:
                    netClient = new TcpClient(iPEnd);
                    break;
#endif
                case NetworkProtocol.WXTCP:
                    break;
                case NetworkProtocol.WXUDP:
                    break;
                default:
                    Debug.LogError($"当前平台不支持该类型 {protocol}");
                    break;
            }
            
            netClient.OnConnectedAct = ()=> {
                closed = false;
                OnConnectedAct?.Invoke();
            };
            netClient.OnMessageAct = SetCryptParam;
            netClient.OnClosedAct = Close;
            closed = true;
        }
        private void SetCryptParam(byte[] data)
        {
            netClient.OnMessageAct = OnMessage;
            using (var memory = new MemoryStream(data))
            using (var reader = new BinaryReader(memory))
            {
                var cryptType = (CryptType)reader.ReadByte();
                var keyLen = reader.ReadByte();
                var key = reader.ReadBytes(keyLen);
                cryptor = cryptType switch
                {
                    CryptType.CryptAes => new AesCryptor(key),
                    CryptType.CryptXor => new XorCryptor(key),
                    _ => NoneCryptor.Default,
                };
                //Debug.Log($"cryptType:{cryptType} keyLen:{keyLen} {BitConverter.ToString(key, 0, key.Length)}");
                int rem = data.Length - keyLen - 2;
                if (rem > 0)
                {
                    byte[] remain = new byte[rem];
                    Buffer.BlockCopy(data, keyLen + 2, remain, 0, rem);
                    OnMessage(remain);
                }
            }
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
