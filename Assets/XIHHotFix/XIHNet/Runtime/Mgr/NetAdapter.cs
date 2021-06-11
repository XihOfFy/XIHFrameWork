using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace XiHNet
{
    public class NetAdapter
    {
        public Queue<Action> ActQue { get; }//等待消费
        public Action OnClosed { get; set; }
        public long ServerTimeUtcTicks => pingpong.ServerTimeUtcTicks;//只有开启pingpong且得到响应后获取的时间才准确

        private readonly NetClient netClient;
        private readonly PBProxy pBProxy;
        private readonly PingPong pingpong;
        private ICryptor cryptor;
        private bool closed;
        public NetAdapter(NetworkProtocol protocol, IPEndPoint iPEnd)
        {
            ActQue = new Queue<Action>();
            pBProxy = new PBProxy();
            pingpong = new PingPong(this, Close);
            switch (protocol)
            {
                case NetworkProtocol.Kcp:
                    netClient = new KcpClient(iPEnd);
                    break;
                case NetworkProtocol.Tcp:
                    netClient = new TcpClient(iPEnd);
                    break;
                default:
                    //TODO 自行添加
                    break;
            }
            closed = true;
        }
        public void StartPingPong()
        {
            pingpong.Start();
        }
        public void RegisterNtf<Ntf>(Action<Ntf> handler) where Ntf : IMessage
        {
            pBProxy.RegisterNtf(handler);
        }
        public async Task<bool> ConnectAsync()
        {
            if (!closed) return true;
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            void RemoveClosedEve()
            {
                netClient.OnClosed -= RemoveClosedEve;
                tcs.SetResult(false);
            }
            netClient.OnMessage = (data) =>
            {
                netClient.OnMessage = OnMessage;
                SetCryptParam(data);
                closed = false;
                netClient.OnClosed -= RemoveClosedEve;
                tcs.SetResult(true);
            };
            netClient.OnClosed = Close;
            netClient.OnClosed += RemoveClosedEve;
            if (await netClient.ConnectAsync())
            {
                return await tcs.Task;
            }
            else
            {
                return false;
            }
        }
        private void SetCryptParam(byte[] data)
        {
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
                Debugger.Log($"cryptType:{cryptType} keyLen:{keyLen} {BitConverter.ToString(key, 0, key.Length)}");
                int rem = data.Length - keyLen - 2;
                if (rem > 0)
                {
                    byte[] remain = new byte[rem];
                    Buffer.BlockCopy(data, keyLen + 2, remain, 0, rem);
                    OnMessage(remain);
                }
            }
        }
        public void Close()
        {
            if (closed) return;
            closed = true;
            pingpong.Stop();
            netClient.Close();
            pBProxy.Dispose();
            ActQue.Clear();
            OnClosed?.Invoke();
        }
        public async Task<IMessage> Request(IMessage req)
        {
            if (closed)
            {
                return NullMessage.IMessageNull;
            }
            Debugger.Log($"Send1111111111");
            (byte[] data, TaskCompletionSource<IMessage> task) = pBProxy.SendReq(req);
            Debugger.Log($"Send22222:{data.Length}");
            cryptor.Encrypt(data, 0, data.Length, out byte[] opt);
            byte[] body = NetConfig.BuildData(opt, req.GetMsgType());
            //Debugger.Log($"客户端发送》》:【{BitConverter.ToString(body, 0, body.Length)}】");
            if (await netClient.RequestAsync(body))
            {
                return await task.Task;
            }
            return NullMessage.IMessageNull;
        }
        public async Task<bool> Notify(IMessage msg)
        {
            if (closed)
            {
                return false;
            }
            byte[] data = pBProxy.SendNtf(msg);
            cryptor.Encrypt(data, 0, data.Length, out byte[] opt);
            byte[] body = NetConfig.BuildData(opt, msg.GetMsgType());
            //Debugger.Log($"客户端发送》》:【{BitConverter.ToString(body, 0, body.Length)}】");
            return await netClient.RequestAsync(body);
        }
        // 构建请求数据,可以将请求信号放在协议头~
        private void OnMessage(byte[] data)
        {
            (bool suc, byte[] rem, ushort msgType, byte[] body) = NetConfig.UnpackBody(data);
            if (suc)
            {
                Debugger.Log($"Rec1111111111");
                cryptor.Decrypt(body, 0, body.Length, out byte[] opt);
                Debugger.Log($"Rec22222:{opt.Length}");
                Action act = pBProxy.DecodeRsp(opt, msgType);
                if (act != null)
                {
                    if (msgType < MsgTypeCodeAttribute.SKIP_PUTIN_QUE)
                    {
                        act();
                    }
                    else
                    {
                        ActQue.Enqueue(act);
                    }
                }
                if (rem != null)
                    OnMessage(rem);
            }
            else
            {
                ActQue.Enqueue(Close);
            }
        }
    }
}
