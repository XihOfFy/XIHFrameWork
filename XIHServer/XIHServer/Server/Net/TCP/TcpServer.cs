
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using XiHNet;

namespace XIHServer
{
    public class TcpServer
    {
        private readonly Socket server;
        private readonly ConcurrentDictionary<string, TcpClientOfServer> clients = new ConcurrentDictionary<string, TcpClientOfServer>();
        private readonly CryptType crypt;
        public readonly Dictionary<ushort, Action<AbsNetClient, byte[]>> handles;
        public TcpServer(IPEndPoint ipEndPoint, CryptType cryptType)
        {
            crypt = cryptType;
            this.server = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            handles = new Dictionary<ushort, Action<AbsNetClient, byte[]>>();
            Debugger.Log($"TcpServer:{ipEndPoint}");
            //this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);//允许将套接字绑定到已在使用中的地址。
            this.server.Bind(ipEndPoint);
            server.Listen(1023);
            LoopRec();
        }
        private async void LoopRec()
        {
            while (true)
            {
                try
                {
                    var sk = await server.AcceptAsync();
                    if (sk == null) return;
                    string key = sk.RemoteEndPoint.ToString();
                    clients.TryRemove(key, out var val);
                    if (val != null)
                    {
                        Debugger.Log($"TCP.TryRemove：{key}客户端重复，将关闭此连接");
                        val.Close();
                    }
                    Debugger.Log("TCP服务器接收对方IP<<: " + key);
                    clients.TryAdd(key, new TcpClientOfServer(sk, crypt, () =>
                    {
                        Debugger.Log($"<color=red>TCP {key} 关闭 </color>");
                        clients.TryRemove(key, out _);
                    }, handles));
                }
                catch (Exception e)
                {
                    Debugger.Log($"<color=red>Tcp服务器承受不住了</color>: {e.ToString()}");
                }
            }
        }
        public void Close()
        {
            server.Close();
            clients.Clear();
        }
    }

    internal class TcpClientOfServer : AbsNetClient
    {
        private readonly Socket client;
        public TcpClientOfServer(Socket sk, CryptType cryptType, Action OnClosed, Dictionary<ushort, Action<AbsNetClient, byte[]>> handles) : base(OnClosed, handles)
        {
            this.client = sk;
            byte[] cryotorKey = Array.Empty<byte>();
            switch (cryptType)
            {
                case CryptType.CryptAes:
                    //cryotorKey = new byte[16] { 76, 156, 114, 226, 131, 245, 190, 137, 241, 141, 178, 42, 44, 19, 153, 128 };
                    cryotorKey = Guid.NewGuid().ToByteArray();
                    cryptor = new AesCryptor(cryotorKey);
                    break;
                case CryptType.CryptXor:
                    //cryotorKey = new byte[] { 1, 2, 3 };
                    cryotorKey = Guid.NewGuid().ToByteArray();
                    cryptor = new XorCryptor(cryotorKey);
                    break;
                case CryptType.CryptNone:
                default:
                    cryptor = NoneCryptor.Default;
                    break;
            }
            Thread th = new Thread(() =>
            {
                if (cryotorKey.Length == 0)
                {
                    sk.Send(new byte[2] { 0x0, 0x0 });
                }
                else
                {
                    byte[] bs = new byte[cryotorKey.Length + 2];
                    Buffer.BlockCopy(cryotorKey, 0, bs, 2, cryotorKey.Length);
                    bs[0] = (byte)cryptType;
                    bs[1] = (byte)cryotorKey.Length;
                    sk.Send(bs);
                }
                try
                {
                    byte[] buff = new byte[NetConfig.BUFFER_SIZE];
                    sk.ReceiveTimeout = NetConfig.RecTimeOut;
                    while (sk.Connected)
                    {
                        int messageLength = sk.Receive(buff);
                        if (messageLength > 0)
                        {
                            byte[] rec = new byte[messageLength];
                            Buffer.BlockCopy(buff, 0, rec, 0, messageLength);
                            OnMsg(rec);
                        }
                        else
                        {
                            Debugger.Log("TCP终端主动断开连接");
                            break;
                        }
                        Thread.Sleep(NetConfig.TcpInterval);
                    }
                }
                catch (Exception e)
                {
                    Debugger.Log(e.ToString());
                }
                Close();
            })
            {
                IsBackground = true
            };
            th.Start();
        }
        public override void Send<Msg>(Msg msg)
        {
            if (isClosed) return;
            client.Send(Map2Bytes(msg));
        }
        public override void Close()
        {
            if (isClosed) return;
            isClosed = true;
            if (AuthStatus != ClientAuth.Replaced)
            {
                MockCache.RemovePlayer(this,Player, SessionKey);
            }
            try
            {
                client.Close();
                client.Dispose();
            }
            catch { }
            onClosedAct?.Invoke();
        }
    }
}
