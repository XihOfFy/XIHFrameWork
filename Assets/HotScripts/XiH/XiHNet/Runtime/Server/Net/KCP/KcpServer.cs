using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using XiHNet;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace XIHServer
{
    public class KcpServer
    {
        private readonly UdpClient server;
        private readonly ConcurrentDictionary<ulong, KcpClientOfServer> clients = new ConcurrentDictionary<ulong, KcpClientOfServer>();
        private readonly ConcurrentDictionary<string, ulong> endpointMap = new ConcurrentDictionary<string, ulong>();
        private readonly CryptType crypt;
        private ulong nextSessionKey = 1;
        private ulong hostSessionKey = 0;
        Thread loopThread;
        public KcpServer(IPEndPoint ipEndPoint, CryptType cryptType)
        {
            crypt = cryptType;
            server = new UdpClient(ipEndPoint);
            Debug.Log($"KcpServer:{ipEndPoint}");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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
            loopThread = new Thread(() =>
            {
                try
                {
                    IPEndPoint recAll = new IPEndPoint(IPAddress.Any, 0);
                    while (true)
                    {
                        if (this.server.Available < 1)
                        {
                            Thread.Sleep(1);
                            continue;
                        }
                        IPEndPoint sender = recAll;
                        byte[] rec = Array.Empty<byte>();
                        try
                        {
                            //Available > 0 时不 sleep，循环排空可用 UDP，避免 OS 内核缓冲丢包
                            rec = this.server.Receive(ref sender);
                            string epKey = sender.ToString();
                            int len = rec.Length;
                            if (len == 0)
                            {
                                if (endpointMap.TryRemove(epKey, out ulong dsk) && clients.TryRemove(dsk, out var val))
                                {
                                    Debug.Log("KCP终端主动断开连接");
                                    BroadcastPlayerLeave(dsk);
                                    val.Close();
                                }
                                continue;
                            }
                            if (len == 1)
                            {
                                if (rec[0] != 0x1)
                                    continue;
                                if (endpointMap.TryRemove(epKey, out ulong oldSk) && clients.TryRemove(oldSk, out var val))
                                {
                                    Debug.Log($"KCP.TryRemove：{epKey}客户端重复，将关闭此连接");
                                    BroadcastPlayerLeave(oldSk);
                                    val.Close();
                                }
                                ulong sk = nextSessionKey++;
                                if (hostSessionKey == 0) hostSessionKey = sk;
                                Debug.Log($"KCP服务器接收对方IP<<: {epKey} SessionKey={sk}");
                                endpointMap.TryAdd(epKey, sk);
                                clients.TryAdd(sk, new KcpClientOfServer(server, sender, crypt, sk, () =>
                                {
                                    Debug.Log($"<color=red>KCP {epKey}(S:{sk}) 关闭 </color>");
                                    endpointMap.TryRemove(epKey, out _);
                                    if (clients.TryRemove(sk, out _))
                                        BroadcastPlayerLeave(sk);
                                }, OnMessage));
                            }
                            else
                            {
                                if (endpointMap.TryGetValue(epKey, out ulong rsk) && clients.TryGetValue(rsk, out var client))
                                    client.RecData(rec);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Log($"<color=red> {sender} 接收异常</color>: {e}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log($"<color=red>Kcp服务器承受不住了</color>: {e}");
                }
            })
            {
                IsBackground = true
            };
            loopThread.Start();
        }
        void OnMessage(AbsNetClient sender, byte[] data)
        {
            //通过 AbsNetClient 的重组缓冲统一切出完整帧，KCP 每包本身就是完整帧，TCP 则做实际重组
            while (true)
            {
                var (success, msgType, route, targetSession, rawBody) = sender.TryPopFrame(data);
                if (!success) return;
                data = null;

                ushort pingType = IMessageExt.GetMsgType<XiHNet.Ping>();
                ushort pongType = IMessageExt.GetMsgType<Pong>();
                if (msgType == pingType)
                {
                    using var inStream = new System.IO.MemoryStream(rawBody);
                    var ping = ProtoBuf.Serializer.Deserialize<XiHNet.Ping>(inStream);
                    var pong = new Pong { TaskId = ping.TaskId, ServerUtcTicks = DateTime.UtcNow.Ticks };
                    using var outStream = new System.IO.MemoryStream();
                    ProtoBuf.Serializer.Serialize(outStream, pong);
                    sender.SendPacked(outStream.ToArray(), pongType);
                }
                else
                {
                    RouteMessage(sender, rawBody, msgType, route, targetSession);
                }
            }
        }
        void RouteMessage(AbsNetClient sender, byte[] rawBody, ushort msgType,
            MsgRoute route, ulong targetSession)
        {
            if (route == MsgRoute.None) route = MsgRoute.Broadcast;

            if ((route & MsgRoute.Broadcast) != 0)
            {
                foreach (var item in clients)
                {
                    if (item.Value == sender) continue;
                    item.Value.SendPacked(rawBody, msgType, route, targetSession);
                }
            }
            if ((route & MsgRoute.ToHost) != 0 && hostSessionKey != 0
                && clients.TryGetValue(hostSessionKey, out var hostClient) && hostClient != sender)
            {
                hostClient.SendPacked(rawBody, msgType, route, targetSession);
            }
            if ((route & MsgRoute.ToSession) != 0 && targetSession != 0)
            {
                if (!clients.TryGetValue(targetSession, out var target) || target == sender)
                    Debug.LogWarning($"[KcpServer] ToSession 未投递 msgType={msgType} targetSession={targetSession} senderSk={sender.SessionKey} 在线数={clients.Count}");
                else
                    target.SendPacked(rawBody, msgType, route, targetSession);
            }
        }
        void BroadcastPlayerLeave(ulong sessionKey)
        {
            var ntf = new PlayerLeaveNtf { SessionKey = sessionKey };
            using var ms = new MemoryStream();
            ProtoBuf.Serializer.Serialize(ms, ntf);
            byte[] body = ms.ToArray();
            ushort msgType = IMessageExt.GetMsgType<PlayerLeaveNtf>();
            foreach (var item in clients)
                item.Value.SendPacked(body, msgType);
        }

        /// <summary>当前 KCP 侧已建立会话列表（含本机 Host LocalClient）。</summary>
        public List<NetPeerSnapshot> GetConnectedPeersSnapshot()
        {
            var list = new List<NetPeerSnapshot>(clients.Count);
            foreach (var kv in clients)
                list.Add(new NetPeerSnapshot(kv.Key, kv.Value.RemoteEndpoint, kv.Key == hostSessionKey));
            list.Sort((a, b) => a.SessionKey.CompareTo(b.SessionKey));
            return list;
        }

        public void Close()
        {
            server.Close();
            clients.Clear();
            endpointMap.Clear();
            loopThread.Abort();
        }
    }
    internal class KcpClientOfServer : AbsNetClient
    {
        private readonly KcpImpl client;
        public string RemoteEndpoint { get; }

        public KcpClientOfServer(UdpClient socket, IPEndPoint remotePoint, CryptType cryptType,
            ulong sessionKey, Action OnClosed, Action<AbsNetClient, byte[]> onMessage) : base(OnClosed, onMessage)
        {
            RemoteEndpoint = remotePoint.ToString();
            SessionKey = sessionKey;
            client = new KcpImpl(remotePoint, socket, Close, OnMsg);
            byte[] cryotorKey = Array.Empty<byte>();
            switch (cryptType)
            {
                case CryptType.CryptAes:
                    cryotorKey = Guid.NewGuid().ToByteArray();
                    cryptor = new AesCryptor(cryotorKey);
                    break;
                case CryptType.CryptXor:
                    cryotorKey = Guid.NewGuid().ToByteArray();
                    cryptor = new XorCryptor(cryotorKey);
                    break;
                case CryptType.CryptNone:
                default:
                    cryptor = NoneCryptor.Default;
                    break;
            }
            byte[] bs = new byte[cryotorKey.Length + 2 + 8];
            bs[0] = (byte)cryptType;
            bs[1] = (byte)cryotorKey.Length;
            if (cryotorKey.Length > 0)
                Buffer.BlockCopy(cryotorKey, 0, bs, 2, cryotorKey.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(sessionKey), 0, bs, cryotorKey.Length + 2, 8);
            client.Send(bs);
        }
        public override void SendMessage2Client(byte[] data)
        {
            if (isClosed) return;
            client.Send(data);
        }
        public void RecData(byte[] data)
        {
            client.PushToRecvQueue(data);
        }
        public override void Close()
        {
            if (isClosed) return;
            isClosed = true;
            client.Close();
            onClosedAct?.Invoke();
        }
        internal class KcpImpl
        {
            private Kcp _kcp;
            //KCP 内部 LinkedList/数组非线程安全。Send 来自 OnMessage 广播的主线程/IO 线程，
            //Input/Update/Recv 来自 StartKcpUpdate 的线程池任务，跨线程并发会触发 LinkedList NRE，
            //导致 update task 静默死亡，整个 sk 永久卡死且无超时无关闭。与 client 端 KcpClient 同款问题。
            private readonly object _kcpLock = new object();
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
                    //Close 流程会先释放 KCP 但 UDP 仍可能在另一线程触发本回调，
                    //SendAsync 可能命中 ObjectDisposedException；async void 异常无人接会污染上层任务。
                    try { await _client.SendAsync(binary, binary.Length, iPEnd); }
                    catch { }
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
                if (_kcp == null) return;
                lock (_kcpLock)
                {
                    if (_kcp == null) return;
                    _kcp.Send(data, 0, data.Length);
                }
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
                //Debug.Log("KCP push error {0}", ex.ToString());
                _errors.Enqueue(ex);
            }
            // dirty read
            private Exception GetError()
            {
                Exception ex = null;
                if (_errors.Count > 0)
                {
                    ex = _errors.Dequeue();
                    Debug.Log($"<color=red>KCP 接收异常</color>: {ex}");
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
                var ex = new TimeoutException($"socket recv timeout {current} - {_lastRecvTime} = {current - _lastRecvTime} <= {_recvTimeoutMM}");
                PushError(ex);
            }
            private void ProcessRecv(uint current)
            {
                var queue = SwitchRecvQueue();
                //Step 1：喂入该 tick 所有 UDP 包，集中更新 KCP 内部重排
                while (queue.Count > 0)
                {
                    _lastRecvTime = current;
                    var data = queue.Dequeue();
                    if (data == null || data.Length == 0) continue;
                    int r;
                    lock (_kcpLock)
                    {
                        if (_kcp == null) return;
                        r = _kcp.Input(data, 0, data.Length);
                    }
                    System.Diagnostics.Debug.Assert(r >= 0);
                    _needUpdate = true;
                }
                //Step 2：排空所有已就绪的业务逻辑包，避免单 tick 只处理一条堆积雪崩
                while (true)
                {
                    int size, r;
                    lock (_kcpLock)
                    {
                        if (_kcp == null) return;
                        size = _kcp.PeekSize();
                        if (size <= 0) break;
                        r = _kcp.Recv(_kcpRcvBuf, 0, _kcpRcvBuf.Length);
                        if (r <= 0) break;
                    }
                    var binary = new byte[size];
                    Buffer.BlockCopy(_kcpRcvBuf, 0, binary, 0, size);
                    //OnMessage 内部会回调 RouteMessage 触发其它 sk 的 _kcp.Send，
                    //严禁持有 _kcpLock 调用 OnMessage，避免与对方 sk 的 Send/Update 形成死锁链
                    OnMessage(binary);
                }
            }
            private void Update(uint current)
            {
                ProcessRecv(current);
                var err = GetError();
                if (err != null)
                {
                    Debug.Log($"<color=red>KCP 接收异常</color>: {err}");
                    Close();
                    return;
                }
                if (_needUpdate || current > _nextUpdateTime)
                {
                    lock (_kcpLock)
                    {
                        if (_kcp == null) return;
                        _kcp.Update(current);
                        _nextUpdateTime = _kcp.Check(current);
                    }
                    _needUpdate = false;
                }
                CheckTimeout(current);
            }
            private void StartKcpUpdate()
            {
                //Task.Factory.StartNew(async ...) 的 async 异常没人 await 会被吞掉，
                //一旦 KCP 内部异常未被 try/catch 兜底，update task 会静默死亡，
                //此后 KCP 永远不再 Update/CheckTimeout，会话永久卡死且无超时无关闭
                Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        while (!closed)
                        {
                            var now = Convert.ToInt64(DateTime.Now.Subtract(new DateTime(2000, 1, 1)).TotalMilliseconds);
                            Update((uint)(now & 0xFFFFFFFF));
                            await Task.Delay(NetConfig.KcpInterval);
                        }
                    }
                    catch (Exception _ex)
                    {
                        Debug.LogError($"<color=red>KCP update task 异常退出</color>: {_ex}");
                        try { Close(); } catch { }
                    }
                });
            }
            public void Close()
            {
                if (closed) return;
                closed = true;
                //Release/置 null 必须与 _kcp.Send/Input/Update/Recv 互斥，
                //否则正在锁内调用 _kcp.* 的线程会拿到悬空指针再次触发 NRE
                lock (_kcpLock)
                {
                    if (_kcp != null)
                    {
                        _kcp.Release();
                        _kcp = null;
                    }
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
