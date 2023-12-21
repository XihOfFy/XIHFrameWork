using System;
using System.IO;
using UnityEngine;

namespace XiHNet
{
    public class NetConfig
    {
        public const byte PKT_HEAD_BYTE = 218;
        public const int HEAD_LEN = 7;
        public const int BUFFER_SIZE = 1024*64;
        public const int KcpInterval = 10;
        public const int TcpInterval = 10;
        public const int RecTimeOut = 10000;//10S
        // 构建消息数据
        public static byte[] BuildData(byte[] body, ushort msgType)
        {
            using var memory = new MemoryStream();
            using var writer = new BinaryWriter(memory);
            writer.Write(NetConfig.PKT_HEAD_BYTE);
            writer.Write(msgType);
            writer.Write((uint)body.Length);
            writer.Write(body);
            return memory.ToArray();
        }
        // 解析消息数据
        public static (bool, byte[], ushort, byte[]) UnpackBody(byte[] data)
        {
            using var memory = new MemoryStream(data);
            using var reader = new BinaryReader(memory);
            byte[] remain = null;
            byte pktMask = reader.ReadByte();
            ushort msgType = reader.ReadUInt16();
            int len = reader.ReadInt32();
            byte[] body = reader.ReadBytes(len);//这里在粘包情况下可能会丢包，Tcp设置NoDelay=true能有效减少粘包
            if (pktMask != NetConfig.PKT_HEAD_BYTE)
            {
                Debug.Log($"<color=green>协议头错误！！直接关闭连接</color>");
                return (false, null, msgType, body);
            }
            int rem = data.Length - len - NetConfig.HEAD_LEN;
            if (rem > 0)
            {
                remain = new byte[rem];
                Buffer.BlockCopy(data, len + NetConfig.HEAD_LEN, remain, 0, rem);
            }
            return (true, remain, msgType, body);
        }
    }
}
