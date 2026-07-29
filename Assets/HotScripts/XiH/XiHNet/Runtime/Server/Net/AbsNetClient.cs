using System;
using System.Collections.Generic;
using XiHNet;

namespace XIHServer
{
    public abstract class AbsNetClient
    {
        protected bool isClosed = false;
        protected readonly Action onClosedAct;
        protected ICryptor cryptor;
        public ulong SessionKey { get; set; }
        private readonly Action<AbsNetClient, byte[]> onMessageAct;
        //TCP 重组缓冲区：单次 Socket.Receive 不保证拿到完整帧，按头部 bodyLen 切帧后再解密分发
        //KCP 每次递送本身就是一个完整帧，走同一路径也幂等
        private readonly List<byte> recvBuffer = new List<byte>(NetConfig.BUFFER_SIZE);
        private readonly object recvLock = new object();

        public AbsNetClient(Action onClosed, Action<AbsNetClient, byte[]> onMessage)
        {
            onClosedAct = onClosed;
            onMessageAct = onMessage;
        }
        public abstract void SendMessage2Client(byte[] data);

        /// <summary>
        /// 追加本次收到的字节（可为 null 表示仅消费已缓冲数据），尝试从缓冲切出下一个完整帧。
        /// 返回 success=false 表示缓冲不足，本轮结束。
        /// </summary>
        public (bool success, ushort msgType, MsgRoute route, ulong targetSession, byte[] rawBody)
            TryPopFrame(byte[] newData)
        {
            lock (recvLock)
            {
                if (newData != null && newData.Length > 0)
                    recvBuffer.AddRange(newData);

                if (recvBuffer.Count < NetConfig.HEAD_LEN)
                    return (false, 0, MsgRoute.None, 0, null);

                if (recvBuffer[0] != NetConfig.PKT_HEAD_BYTE)
                {
                    UnityEngine.Debug.LogError($"[AbsNetClient S:{SessionKey}] 协议头损坏，断开连接");
                    Close();
                    return (false, 0, MsgRoute.None, 0, null);
                }

                ushort msgType = (ushort)(recvBuffer[1] | (recvBuffer[2] << 8));
                MsgRoute route = (MsgRoute)recvBuffer[3];
                byte[] tsBytes = new byte[8];
                for (int i = 0; i < 8; ++i) tsBytes[i] = recvBuffer[4 + i];
                ulong targetSession = BitConverter.ToUInt64(tsBytes, 0);
                int bodyLen = recvBuffer[12] | (recvBuffer[13] << 8)
                    | (recvBuffer[14] << 16) | (recvBuffer[15] << 24);
                if (bodyLen < 0 || bodyLen > NetConfig.BUFFER_SIZE * 8)
                {
                    UnityEngine.Debug.LogError($"[AbsNetClient S:{SessionKey}] 异常 bodyLen={bodyLen}，断开连接");
                    Close();
                    return (false, 0, MsgRoute.None, 0, null);
                }
                int total = NetConfig.HEAD_LEN + bodyLen;
                if (recvBuffer.Count < total)
                    return (false, 0, MsgRoute.None, 0, null);//等尾部字节补齐

                byte[] body = new byte[bodyLen];
                for (int i = 0; i < bodyLen; ++i) body[i] = recvBuffer[NetConfig.HEAD_LEN + i];
                recvBuffer.RemoveRange(0, total);

                cryptor.Decrypt(body, 0, body.Length, out byte[] rawBody);
                return (true, msgType, route, targetSession, rawBody);
            }
        }

        public void SendPacked(byte[] rawBody, ushort msgType,
            MsgRoute route = MsgRoute.None, ulong targetSession = 0)
        {
            if (isClosed) return;
            cryptor.Encrypt(rawBody, 0, rawBody.Length, out byte[] encrypted);
            SendMessage2Client(NetConfig.BuildData(encrypted, msgType, route, targetSession));
        }

        protected void OnMsg(byte[] data)
        {
            if (isClosed) return;
            onMessageAct.Invoke(this, data);
        }
        public abstract void Close();
    }
}
