using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace XiHNet
{
    public class TcpClient : NetClient
    {
        private System.Net.Sockets.TcpClient _client;
        private NetworkStream networkStream;
        public TcpClient(IPEndPoint iPEnd) : base(iPEnd)
        {
        }
        public override async void Connect()
        {
            try
            {
                if (NetState == NetState.Open)
                {
                    return;
                }
                _client = new System.Net.Sockets.TcpClient(_endPoint.AddressFamily)
                {
                    LingerState = new LingerOption(true, 0),
                    NoDelay = true,//不会粘包，但可能数据大会分包，所以设置需要不允许最大发送大小
                    SendBufferSize = NetConfig.BUFFER_SIZE,
                    ReceiveBufferSize = NetConfig.BUFFER_SIZE,
                    SendTimeout = 0,
                    ReceiveTimeout = NetConfig.RecTimeOut
                };
                await Task.Factory.StartNew(() =>
                {
                    _client.ConnectAsync(_endPoint.Address, _endPoint.Port).Wait(NetConfig.RecTimeOut);
                });
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
        private CancellationTokenSource source;
        private async void StartRecUpdate()
        {
            source = new CancellationTokenSource();
            await await Task.Factory.StartNew(async () =>
            {
                try
                {
                    using (networkStream = _client.GetStream())
                    {
                        networkStream.ReadTimeout = NetConfig.RecTimeOut;
                        byte[] bs = new byte[NetConfig.BUFFER_SIZE];
                        while (NetState == NetState.Open)
                        {
                            int len = await networkStream.ReadAsync(bs, 0, NetConfig.BUFFER_SIZE);
                            if (len <= 0)
                            {
                                Debug.Log("终端主动断开连接");
                                break;
                            }
                            byte[] arr = new byte[len];
                            Buffer.BlockCopy(bs, 0, arr, 0, len);
                            OnMessageAct.Invoke(arr);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            });
            Close();
        }

        public async override void Request(byte[] data)
        {
            if (NetState != NetState.Open)
            {
                return;
            }
            try
            {
                await networkStream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
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
            source.Cancel();
            source.Dispose();
            OnClosedAct?.Invoke();
        }
    }
}
