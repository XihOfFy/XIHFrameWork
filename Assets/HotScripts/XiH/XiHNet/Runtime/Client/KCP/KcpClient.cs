#if !UNITY_WEBGL || UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace XiHNet
{
    public class KcpClient : NetClient
    {
        private Kcp _kcp;
        //KCP 内部使用 LinkedList/数组等非线程安全结构。Request 在线程池跑 _kcp.Send，
        //同时 StartKcpUpdate 在另一个线程池任务里跑 _kcp.Input/Update/PeekSize/Recv，
        //并发修改会导致 LinkedList 节点指针损坏 → AddLast 内部 NRE → catch → Close 掉线。
        //用一把锁串行化所有 _kcp.* 调用即可。
        private readonly object _kcpLock = new object();
        private UdpClient _client = null;
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
        public KcpClient(IPEndPoint iPEnd) : base(iPEnd)
        {
            _recvTimeoutMM = NetConfig.RecTimeOut;
            _kcpRcvBuf = new byte[(Kcp.IkcpMtuDef + Kcp.IkcpOverhead) * 3];
            _rcvQueue = new Queue<byte[]>(64);
            _forGround = new Queue<byte[]>(64);
            _errors = new Queue<Exception>(8);
        }
        public override async void Connect()
        {
            try
            {
                if (NetState == NetState.Open)
                {
                    return;
                }
                this._client = new UdpClient();// { ExclusiveAddressUse=true};
                await Task.Factory.StartNew(() =>
                {
                    _client.Client.ReceiveTimeout = NetConfig.RecTimeOut;
                    _client.Connect(_endPoint);
                });
                await _client.SendAsync(new byte[] { 0x1 }, 1);
                _kcp = new Kcp(912, OutputKcpAsync);
                // fast mode
                _kcp.NoDelay(1, 10, 2, 1);
                _kcp.WndSize(1024, 1024);
                NetState = NetState.Open;
                StartRecUpdate();
                OnConnectedAct?.Invoke();
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                Close();
            }
        }
        private async void OutputKcpAsync(byte[] data, int size)
        {
            if (NetState != NetState.Open)
            {
                return;
            }
            var binary = new byte[size];
            Buffer.BlockCopy(data, 0, binary, 0, size);
            //Close 流程会先 Dispose UdpClient 再释放 KCP，KCP 内部仍可能在另一线程触发本回调，
            //此时 SendAsync 会抛 ObjectDisposedException；async void 异常无人接会污染上层任务。
            try { await _client.SendAsync(binary, binary.Length); }
            catch { }
        }
        private CancellationTokenSource source;
        private async void StartRecUpdate()
        {
            source = new CancellationTokenSource();
            await await Task.Factory.StartNew(async () =>
            {
                try
                {
                    StartKcpUpdate();
                    while (NetState == NetState.Open)
                    {
                        //每次轮询把当前 UDP 缓冲排空，避免 OS 级缓冲溢出导致丢包、KCP 重传风暴和超时
                        while (_client != null && _client.Available > 0)
                        {
                            UdpReceiveResult res = await _client.ReceiveAsync();
                            var data = res.Buffer;
                            if (data.Length <= 0)
                            {
                                Debug.Log("终端主动断开连接");
                                PushToRecvQueue(Array.Empty<byte>());
                                NetState = NetState.Closed;
                                break;
                            }
                            PushToRecvQueue(data);
                        }
                        await Task.Delay(NetConfig.KcpInterval >> 1);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }
            }, source.Token);
            Close();
        }

        private void PushToRecvQueue(byte[] data)
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
        // 业务消息发送事件，进入 KCP 模块
        public override async void Request(byte[] data)
        {
            if (_kcp == null)
            {
                return;
            }
            var ret = -1;
            await Task.Factory.StartNew(() =>
            {
                lock (_kcpLock)
                {
                    if (_kcp == null) return;
                    ret = _kcp.Send(data, 0, data.Length);
                }
            });
            _needUpdate = true;
            if (ret == 0)
            {
                return;
            }
            else
            {
                return;
            }
        }
        private void ProcessRecv(uint current)
        {
            var queue = SwitchRecvQueue();
            //Step 1：先把当前批次所有 UDP 包喂入 KCP，集中处理 ACK/重组
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
                Debug.Assert(r >= 0);
                _needUpdate = true;
            }
            //Step 2：循环取完所有已就绪的逻辑消息，防止一次 tick 只能消费一条导致延迟堆积
            if (NetState != NetState.Open) return;
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
                //OnMessageAct 内部有自己的 lock(recvLock)，不应再持 _kcpLock，避免锁顺序耦合
                OnMessageAct.Invoke(binary);
            }
        }
        private bool Update(uint current)
        {
            ProcessRecv(current);
            if (_needUpdate || current > _nextUpdateTime)
            {
                lock (_kcpLock)
                {
                    if (_kcp == null) return false;
                    _kcp.Update(current);
                    _nextUpdateTime = _kcp.Check(current);
                    _needUpdate = false;
                }
            }
            return current - _lastRecvTime <= _recvTimeoutMM;
        }

        private async void StartKcpUpdate()
        {
            await await Task.Factory.StartNew(async () =>
            {
                DateTime d2 = new DateTime(2000, 1, 1);
                var now = Convert.ToInt64(DateTime.Now.Subtract(d2).TotalMilliseconds);
                _lastRecvTime = (uint)(now & 0xFFFFFFFF);
                while (NetState == NetState.Open)
                {
                    now = Convert.ToInt64(DateTime.Now.Subtract(d2).TotalMilliseconds);
                    if (Update((uint)(now & 0xFFFFFFFF)))
                    {
                        await Task.Delay(NetConfig.KcpInterval);
                    }
                    else
                    {
                        break;
                    }
                }
            }, source.Token);
            Close();
        }

        public override void Close()
        {
            if (NetState == NetState.Closed)
                return;
            NetState = NetState.Closed;
            if (_client != null)
            {
                try
                {
                    _client.Close();
                    _client.Dispose();
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
                _client = null;
            }
            lock (_kcpLock)
            {
                if (_kcp != null)
                {
                    _kcp.Release();
                    _kcp = null;
                }
            }
            source.Cancel();
            source.Dispose();
            _lastRecvTime = 0;
            _errors.Clear();
            _forGround.Clear();
            _rcvQueue.Clear();
            OnClosedAct?.Invoke();
        }
    }
}

#endif
