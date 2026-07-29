
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using XiHNet;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace XIHServer
{
    public class TcpServer
    {
        private readonly Socket server;
        private readonly ConcurrentDictionary<ulong, TcpClientOfServer> clients = new ConcurrentDictionary<ulong, TcpClientOfServer>();
        private readonly CryptType crypt;
        private ulong nextSessionKey = 1;
        private ulong hostSessionKey = 0;
        private bool loop;
        public TcpServer(IPEndPoint ipEndPoint, CryptType cryptType)
        {
            crypt = cryptType;
            this.server = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.server.Bind(ipEndPoint);
            server.Listen(1023);
            loop = true;
            LoopRec();
        }
        private async void LoopRec()
        {
            while (loop)
            {
                try
                {
                    var sk = await server.AcceptAsync();
                    if (sk == null) return;
                    string ep = sk.RemoteEndPoint.ToString();
                    ulong sessionKey = nextSessionKey++;
                    if (hostSessionKey == 0) hostSessionKey = sessionKey;
                    Debug.Log($"TCP服务器接收对方IP<<: {ep} SessionKey={sessionKey}");
                    clients.TryAdd(sessionKey, new TcpClientOfServer(sk, crypt, sessionKey, () =>
                    {
                        Debug.Log($"<color=red>TCP {ep}(S:{sessionKey}) 关闭 </color>");
                        if (clients.TryRemove(sessionKey, out _))
                            BroadcastPlayerLeave(sessionKey);
                    }, OnMessage));
                }
                catch (Exception e)
                {
                    Debug.Log($"<color=red>Tcp服务器承受不住了</color>: {e.ToString()}");
                }
            }
        }
        void OnMessage(AbsNetClient sender, byte[] data)
        {
            //TCP 递送的 data 可能是半帧 / 多帧拼包，由 AbsNetClient 的重组缓冲统一切出完整帧；
            //单次 Receive 里的所有完整帧一轮内处理完，避免积压
            while (true)
            {
                var (success, msgType, route, targetSession, rawBody) = sender.TryPopFrame(data);
                if (!success) return;//或尚未补齐或已经消费完，等下次 Receive
                data = null;//后续轮次不再追加原始字节，仅从缓冲继续取

                if (msgType == IMessageExt.GetMsgType<XiHNet.Ping>())
                {
                    using var inStream = new System.IO.MemoryStream(rawBody);
                    var ping = ProtoBuf.Serializer.Deserialize<XiHNet.Ping>(inStream);
                    var pong = new Pong { TaskId = ping.TaskId, ServerUtcTicks = DateTime.UtcNow.Ticks };
                    using var outStream = new System.IO.MemoryStream();
                    ProtoBuf.Serializer.Serialize(outStream, pong);
                    sender.SendPacked(outStream.ToArray(), IMessageExt.GetMsgType<Pong>());
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
                    Debug.LogWarning($"[TcpServer] ToSession 未投递 msgType={msgType} targetSession={targetSession} senderSk={sender.SessionKey} 在线数={clients.Count}");
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

        /// <summary>当前 TCP 侧已建立会话列表（含本机 Host LocalClient）。</summary>
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
            loop = false;
        }
    }

    internal class TcpClientOfServer : AbsNetClient
    {
        private readonly Socket client;
        public string RemoteEndpoint { get; }

        public TcpClientOfServer(Socket sk, CryptType cryptType, ulong sessionKey,
            Action OnClosed, Action<AbsNetClient, byte[]> onMessage) : base(OnClosed, onMessage)
        {
            RemoteEndpoint = sk.RemoteEndPoint?.ToString() ?? "";
            SessionKey = sessionKey;
            this.client = sk;
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
            Thread th = new Thread(() =>
            {
                byte[] bs = new byte[cryotorKey.Length + 2 + 8];
                bs[0] = (byte)cryptType;
                bs[1] = (byte)cryotorKey.Length;
                if (cryotorKey.Length > 0)
                    Buffer.BlockCopy(cryotorKey, 0, bs, 2, cryotorKey.Length);
                Buffer.BlockCopy(BitConverter.GetBytes(sessionKey), 0, bs, cryotorKey.Length + 2, 8);
                sk.Send(bs);
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
                            Debug.Log("TCP终端主动断开连接");
                            break;
                        }
                        Thread.Sleep(NetConfig.TcpInterval);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }
                Close();
            })
            {
                IsBackground = true
            };
            th.Start();
        }
        public override void SendMessage2Client(byte[] data)
        {
            if (isClosed) return;
            client.Send(data);
        }
        public override void Close()
        {
            if (isClosed) return;
            isClosed = true;
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
