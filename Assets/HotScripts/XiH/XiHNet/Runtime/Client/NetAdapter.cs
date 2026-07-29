using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace XiHNet
{
    public partial class NetAdapter
    {
        public Action OnConnectedAct { get; set; }
        public Action OnClosedAct { get; set; }
        /// <summary>
        /// 服务器分配的会话标识，连接成功后可用
        /// </summary>
        public ulong SessionKey { get; private set; }
        public readonly NetClient netClient;
        private ICryptor cryptor;
        private bool closed;
        //TCP 流式传输不保证每次 OnMessageAct 拿到完整帧（可能拆/合包），
        //这里统一按头部 bodyLen 字段重组帧。KCP 每次递送本就是完整帧，走同一路径也幂等。
        private readonly List<byte> recvBuffer = new List<byte>(NetConfig.BUFFER_SIZE);
        private bool handshakeConsumed;
        //所有网络回调来自 IO 线程，这里用锁保护 recvBuffer / handshakeConsumed，
        //避免再接到回调时与 Close 中状态清理冲突
        private readonly object recvLock = new object();

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
            pbProxy = new PBProxy();
            netClient.OnConnectedAct = () =>
            {
                closed = false;
            };
            netClient.OnMessageAct = OnRawData;
            netClient.OnClosedAct = Close;
            closed = true;
        }

        /// <summary>
        /// 所有底层网络包（handshake + framed）统一入口：
        /// 累积到 recvBuffer 后循环消费，直到无完整帧或 handshake 才退出。
        /// 一个 TCP read 可能包含半帧、多帧或 handshake+N 帧拼在一起，必须在这里重组。
        /// </summary>
        private void OnRawData(byte[] data)
        {
            if (data == null || data.Length == 0) return;
            lock (recvLock)
            {
                recvBuffer.AddRange(data);
                try
                {
                    if (!handshakeConsumed)
                    {
                        if (!TryConsumeHandshake()) return;
                        handshakeConsumed = true;
                    }
                    ConsumeFrames();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[NetAdapter] 接收处理异常，关闭连接: {e}");
                    Close();
                }
            }
        }

        /// <summary>
        /// 从 recvBuffer 尝试解析 handshake（非帧结构）：
        /// [cryptType:1][keyLen:1][key:keyLen][sessionKey:8]
        /// 不够完整就先等下次 OnRawData。
        /// </summary>
        private bool TryConsumeHandshake()
        {
            if (recvBuffer.Count < 2) return false;
            int keyLen = recvBuffer[1];
            int need = 2 + keyLen + 8;
            if (recvBuffer.Count < need) return false;

            var cryptType = (CryptType)recvBuffer[0];
            byte[] key = new byte[keyLen];
            for (int i = 0; i < keyLen; ++i) key[i] = recvBuffer[2 + i];
            cryptor = cryptType switch
            {
                CryptType.CryptAes => new AesCryptor(key),
                CryptType.CryptXor => new XorCryptor(key),
                _ => NoneCryptor.Default,
            };
            byte[] skBytes = new byte[8];
            for (int i = 0; i < 8; ++i) skBytes[i] = recvBuffer[2 + keyLen + i];
            SessionKey = BitConverter.ToUInt64(skBytes, 0);

            recvBuffer.RemoveRange(0, need);
            processQueue.Enqueue(OnConnectedAct);
            return true;
        }

        /// <summary>
        /// 持续从 recvBuffer 切出完整帧。帧头格式固定 HEAD_LEN 字节，
        /// 其中 offset 12 处为 uint32 bodyLen。长度不足则等待下次数据。
        /// </summary>
        private void ConsumeFrames()
        {
            while (recvBuffer.Count >= NetConfig.HEAD_LEN)
            {
                if (recvBuffer[0] != NetConfig.PKT_HEAD_BYTE)
                {
                    Debug.LogError("[NetAdapter] 协议头损坏，丢弃连接");
                    Close();
                    return;
                }
                ushort msgType = (ushort)(recvBuffer[1] | (recvBuffer[2] << 8));
                var frameRoute = (MsgRoute)recvBuffer[3];
                var tsFrame = new byte[8];
                for (int i = 0; i < 8; i++) tsFrame[i] = recvBuffer[4 + i];
                ulong frameTargetSession = BitConverter.ToUInt64(tsFrame, 0);
                int bodyLen = recvBuffer[12] | (recvBuffer[13] << 8) | (recvBuffer[14] << 16) | (recvBuffer[15] << 24);
                if (bodyLen < 0 || bodyLen > NetConfig.BUFFER_SIZE * 8)
                {
                    Debug.LogError($"[NetAdapter] 异常 bodyLen={bodyLen}，断开连接");
                    Close();
                    return;
                }
                int total = NetConfig.HEAD_LEN + bodyLen;
                if (recvBuffer.Count < total) return;//等尾部字节补齐

                byte[] body = new byte[bodyLen];
                for (int i = 0; i < bodyLen; ++i) body[i] = recvBuffer[NetConfig.HEAD_LEN + i];
                recvBuffer.RemoveRange(0, total);

                cryptor.Decrypt(body, 0, body.Length, out byte[] opt);
                PBProxyOnMessage(opt, msgType, frameRoute, frameTargetSession);
            }
        }

        public void Connect()
        {
            if (!closed) return;
            lock (recvLock)
            {
                recvBuffer.Clear();
                handshakeConsumed = false;
            }
            netClient.Connect();
        }
        public void Close()
        {
            if (closed) return;
            closed = true;
            netClient.Close();
            pbProxy.Dispose();
            lock (recvLock)
            {
                recvBuffer.Clear();
                handshakeConsumed = false;
            }
            //严禁在接收线程上直接触发关闭回调（其链上包含 Destroy、Singleton 等主线程 API），
            //统一入 processQueue，由 Unity 主线程 Update 时消费
            var act = OnClosedAct;
            if (act != null) processQueue.Enqueue(act);
        }
        private void Request(byte[] data, ushort msgType,
            MsgRoute route = MsgRoute.None, ulong targetSession = 0)
        {
            if (closed) return;
            cryptor.Encrypt(data, 0, data.Length, out byte[] opt);
            netClient.Request(NetConfig.BuildData(opt, msgType, route, targetSession));
        }
    }
}
