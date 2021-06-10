using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using XiHNet;
namespace XIHServer
{
    public class KcpServer
    {
        private readonly UdpClient server;
        private readonly ConcurrentDictionary<string, KcpClientOfServer> clients = new ConcurrentDictionary<string, KcpClientOfServer>();
        private readonly CryptType crypt;
        public readonly Dictionary<ushort, Action<AbsNetClient, byte[]>> handles;
        public KcpServer(IPEndPoint ipEndPoint, CryptType cryptType)
        {
            crypt = cryptType;
            server = new UdpClient(ipEndPoint);
            handles = new Dictionary<ushort, Action<AbsNetClient, byte[]>>();
            Debugger.Log($"KcpServer:{ipEndPoint}");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))//远程主机强迫关闭了一个现有的连接0x80004005
            {
                const uint IOC_IN = 0x80000000;
                const uint IOC_VENDOR = 0x18000000;
                uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                this.server.Client.IOControl((int)SIO_UDP_CONNRESET, new[] { Convert.ToByte(false) }, null);
            }
            LoopRec();
        }
        private void LoopRec()
        {
            Thread th = new Thread(() =>
            {
                try
                {
                    IPEndPoint recAll = new IPEndPoint(IPAddress.Any, 0);
                    while (true)
                    {
                        Thread.Sleep(1);
                        if (this.server.Available < 1) continue;
                        IPEndPoint sender = recAll;
                        byte[] rec = Array.Empty<byte>();
                        try
                        {
                            rec = this.server.Receive(ref sender);
                            string curKey = sender.ToString();
                            // 长度小于1，不是正常的消息
                            int len = rec.Length;
                            if (len == 0)
                            {
                                //终端主动端口连接
                                clients.TryRemove(curKey, out var val);
                                if (val != null)
                                {
                                    Debugger.Log("KCP终端主动断开连接");
                                    val.Close();
                                }
                                continue;
                            }
                            //Debugger.Log($"<color=red>服务端接收</color>《《{curKey}:【{BitConverter.ToString(rec, 0, rec.Length)}】");
                            if (len == 1)
                            {
                                if (rec[0] != 0x1)
                                    continue;
                                //请求加密不走KCP模式
                                clients.TryRemove(curKey, out var val);
                                if (val != null)
                                {
                                    Debugger.Log($"KCP.TryRemove：{curKey}客户端重复，将关闭此连接");
                                    val.Close();
                                }
                                Debugger.Log("KCP服务器接收对方IP<<: " + curKey);
                                clients.TryAdd(curKey, new KcpClientOfServer(server, sender, crypt, () =>
                                {
                                    Debugger.Log($"<color=red>KCP {curKey} 关闭 </color>");
                                    clients.TryRemove(curKey, out _);
                                }, handles));
                            }
                            else
                            {//KCP模式
                                if (clients.TryGetValue(curKey, out var client)) client.RecData(rec);
                            }
                        }
                        catch (Exception e)
                        {
                            Debugger.Log($"<color=red> {sender} 接收异常</color>: {e}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debugger.Log($"<color=red>Kcp服务器承受不住了</color>: {e}");
                }
            })
            {
                IsBackground = true
            };
            th.Start();
        }
        public void Close()
        {
            server.Close();
            clients.Clear();
        }
    }
    internal class KcpClientOfServer : AbsNetClient
    {
        private readonly KcpImpl client;
        public KcpClientOfServer(UdpClient socket, IPEndPoint remotePoint, CryptType cryptType, Action OnClosed, Dictionary<ushort, Action<AbsNetClient, byte[]>> handles) : base(OnClosed, handles)
        {
            client = new KcpImpl(remotePoint, socket, Close, OnMsg);
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
            if (cryotorKey.Length == 0)
            {
                client.Send(new byte[2] { 0x0, 0x0 });
            }
            else
            {
                byte[] bs = new byte[cryotorKey.Length + 2];
                Buffer.BlockCopy(cryotorKey, 0, bs, 2, cryotorKey.Length);
                bs[0] = (byte)cryptType;
                bs[1] = (byte)cryotorKey.Length;
                //Debugger.Log($"{BitConverter.ToString(bs, 0, bs.Length)}");
                client.Send(bs);//返回加密信息，开始走KCP模式
            }
        }
        public override void Send<Msg>(Msg msg)
        {
            if (isClosed) return;
            client.Send(Map2Bytes(msg));
        }
        public void RecData(byte[] data)
        {
            client.PushToRecvQueue(data);
        }
        public override void Close()
        {
            if (isClosed) return;
            isClosed = true;
            if (AuthStatus != ClientAuth.Replaced)
            {
                MockCache.RemovePlayer(this, Player, SessionKey);
            }
            client.Close();
            onClosedAct?.Invoke();
        }
        internal class KcpImpl
        {
            private Kcp _kcp;
            private UdpClient _client = null;
            private readonly Action<byte[]> OnMessage;
            private readonly Action OnClosed;
            private bool closed = false;
            // recv buffer
            private readonly byte[] _kcpRcvBuf;
            private Queue<byte[]> _rcvQueue;
            private Queue<byte[]> _forGround;
            private readonly Queue<Exception> _errors;
            // time-out control
            private long _lastRecvTime = 0;
            private readonly int _recvTimeoutMM = 0;
            private bool _needUpdate = false;
            private uint _nextUpdateTime = 0;
            public KcpImpl(IPEndPoint iPEnd, UdpClient client, Action onClosed, Action<byte[]> onMessage)
            {
                _recvTimeoutMM = NetConfig.RecTimeOut;
                _kcpRcvBuf = new byte[(Kcp.IkcpMtuDef + Kcp.IkcpOverhead) * 3];
                _rcvQueue = new Queue<byte[]>(64);
                _forGround = new Queue<byte[]>(64);
                _errors = new Queue<Exception>(8);
                this._client = client;
                _kcp = new Kcp(912, async (data, size) =>
                {
                    var binary = new byte[size];
                    Buffer.BlockCopy(data, 0, binary, 0, size);
                    //Debugger.Log($"{BitConverter.ToString(data, 0, size)}");
                    await _client.SendAsync(binary, binary.Length, iPEnd);
                });
                // fast mode
                _kcp.NoDelay(1, 10, 2, 1);
                _kcp.WndSize(1024, 1024);
                StartKcpUpdate();
                this.OnClosed = onClosed;
                this.OnMessage = onMessage;
            }
            // 业务消息发送事件，进入 KCP 模块
            public void Send(byte[] data)
            {
                if (_kcp == null)
                {
                    return;
                }
                _kcp.Send(data, 0, data.Length);
                _needUpdate = true;
            }
            public void PushToRecvQueue(byte[] data)
            {
                lock (_rcvQueue)
                {
                    _rcvQueue.Enqueue(data);
                }
            }
            // if `rcvqueue` is not empty, swap it with `forground`
            private Queue<byte[]> SwitchRecvQueue()
            {
                lock (_rcvQueue)
                {
                    if (_rcvQueue.Count <= 0) return _forGround;
                    var tmp = _rcvQueue;
                    _rcvQueue = _forGround;
                    _forGround = tmp;
                }
                return _forGround;
            }
            // dirty write
            private void PushError(Exception ex)
            {
                // Debugger.Log("KCP push error {0}", ex.ToString());
                _errors.Enqueue(ex);
            }
            // dirty read
            private Exception GetError()
            {
                Exception ex = null;
                if (_errors.Count > 0)
                {
                    ex = _errors.Dequeue();
                }
                return ex;
            }
            private void CheckTimeout(uint current)
            {
                if (_lastRecvTime == 0)
                {
                    _lastRecvTime = current;
                }

                if (current - _lastRecvTime <= _recvTimeoutMM) return;
                var ex = new TimeoutException("socket recv timeout");
                PushError(ex);
            }
            private void ProcessRecv(uint current)
            {
                var queue = SwitchRecvQueue();
                while (queue.Count > 0)
                {
                    _lastRecvTime = current;
                    var data = queue.Dequeue();
                    var r = _kcp.Input(data, 0, data.Length);
                    System.Diagnostics.Debug.Assert(r >= 0);
                    _needUpdate = true;
                    var size = _kcp.PeekSize();
                    if (size > 0)
                    {
                        r = _kcp.Recv(_kcpRcvBuf, 0, _kcpRcvBuf.Length);
                        if (r <= 0)
                        {
                            break;
                        }
                        var binary = new byte[size];
                        Buffer.BlockCopy(_kcpRcvBuf, 0, binary, 0, size);
                        OnMessage(binary);
                    }
                }
            }
            private void Update(uint current)
            {
                ProcessRecv(current);
                var err = GetError();
                if (err != null)
                {
                    Close();
                    return;
                }
                if (_needUpdate || current > _nextUpdateTime)
                {
                    _kcp.Update(current);
                    _nextUpdateTime = _kcp.Check(current);
                    _needUpdate = false;
                }
                CheckTimeout(current);
            }
            private void StartKcpUpdate()
            {
                Task.Factory.StartNew(async () =>
                {
                    while (!closed)
                    {
                        var now = Convert.ToInt64(DateTime.Now.Subtract(new DateTime(2000, 1, 1)).TotalMilliseconds);
                        Update((uint)(now & 0xFFFFFFFF));
                        await Task.Delay(NetConfig.KcpInterval);
                    }
                });
            }
            public void Close()
            {
                if (closed) return;
                closed = true;
                if (_kcp != null)
                {
                    _kcp.Release();
                    _kcp = null;
                }
                _lastRecvTime = 0;
                _errors.Clear();
                _forGround.Clear();
                _rcvQueue.Clear();
                OnClosed();
            }
        }
    }
}
