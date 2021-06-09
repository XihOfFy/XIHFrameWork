using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace XiHNet
{
    public class KcpClient : NetClient
    {
        private Kcp _kcp;
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
        public override async Task<bool> ConnectAsync()
        {
            try
            {
                if (NetState == NetState.Open)
                {
                    return true;
                }
                this._client = new UdpClient();// { ExclusiveAddressUse=true};
                await Task.Factory.StartNew(() =>
                {
                    _client.Client.ReceiveTimeout = NetConfig.RecTimeOut;
                    _client.Connect(_endPoint);
                });
                _kcp = new Kcp(912, OutputKcpAsync);
                // fast mode
                _kcp.NoDelay(1, 10, 2, 1);
                _kcp.WndSize(1024, 1024);
                NetState = NetState.Open;
                StartRecUpdate();
                //return await SendAsync(new byte[1] { 0x1 });//请求加密走KCP模式
                return await _client.SendAsync(new byte[1] { 0x1 }, 1) > 0;//请求加密不走KCP模式
            }
            catch (Exception e)
            {
                Debugger.Log(e.ToString());
            }
            Close();
            return false;
        }
        private async void OutputKcpAsync(byte[] data, int size)
        {
            if (NetState != NetState.Open)
            {
                return;
            }
            var binary = new byte[size];
            Buffer.BlockCopy(data, 0, binary, 0, size);
            await _client.SendAsync(binary, binary.Length);
            //Debugger.Log($"OutputKcpAsync <color=red>{_udpClient.Client.LocalEndPoint}  | {_endPoint} | {BitConverter.ToString(data,0,size)}</color> ");
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
                        if (_client.Available > 0)
                        {
                            UdpReceiveResult res = await _client.ReceiveAsync();
                            var data = res.Buffer;
                            if (data.Length <= 0)
                            {
                                Debugger.Log("终端主动断开连接");
                                break;
                            }
                            PushToRecvQueue(data);
                        }
                        await Task.Delay(NetConfig.KcpInterval >> 1);
                    }
                }
                catch (Exception e)
                {
                    Debugger.Log(e.ToString());
                }
            },source.Token);
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
        public override async Task<bool> RequestAsync(byte[] data)
        {
            if (_kcp == null)
            {
                return false;
            }
            var ret = -1;
            await Task.Factory.StartNew(() =>
            {
                ret = _kcp.Send(data, 0, data.Length);
            });
            _needUpdate = true;
            if (ret == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void ProcessRecv(uint current)
        {
            var queue = SwitchRecvQueue();
            while (queue.Count > 0)
            {
                _lastRecvTime = current;
                var data = queue.Dequeue();
                var r = _kcp.Input(data, 0, data.Length);
                Debug.Assert(r >= 0);
                _needUpdate = true;
                if (NetState == NetState.Open)
                {
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
                        OnMessage.Invoke(binary);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        private bool Update(uint current)
        {
            ProcessRecv(current);
            if (_needUpdate || current > _nextUpdateTime)
            {
                _kcp.Update(current);
                _nextUpdateTime = _kcp.Check(current);
                _needUpdate = false;
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
                    else {
                        break;
                    }
                }
            },source.Token);
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
                    Debugger.Log(e.Message);
                }
                _client = null;
            }
            if (_kcp != null)
            {
                _kcp.Release();
                _kcp = null;
            }
            source.Cancel();
            source.Dispose();
            _lastRecvTime = 0;
            _errors.Clear();
            _forGround.Clear();
            _rcvQueue.Clear();
            OnClosed?.Invoke();
        }
    }
}