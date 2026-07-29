using System;
using System.IO;
using UnityEngine;

namespace XiHNet
{
    public class NetConfig
    {
        public const byte PKT_HEAD_BYTE = 218;
        /// <summary>
        /// 包头长度: HEAD(1) + msgType(2) + route(1) + targetSession(8) + bodyLen(4) = 16
        /// </summary>
        public const int HEAD_LEN = 16;
        public const int BUFFER_SIZE = 1024 * 64;
        public const int KcpInterval = 10;
        public const int TcpInterval = 10;
        public const int RecTimeOut = 100000;//100S

        public static byte[] BuildData(byte[] body, ushort msgType,
            MsgRoute route = MsgRoute.None, ulong targetSession = 0)
        {
            using var memory = new MemoryStream();
            using var writer = new BinaryWriter(memory);
            writer.Write(PKT_HEAD_BYTE);
            writer.Write(msgType);
            writer.Write((byte)route);
            writer.Write(targetSession);
            writer.Write((uint)body.Length);
            writer.Write(body);
            return memory.ToArray();
        }

        public static (bool suc, byte[] remain, ushort msgType,
            MsgRoute route, ulong targetSession, byte[] body) UnpackBody(byte[] data)
        {
            if (data.Length < HEAD_LEN)
                return (false, null, 0, MsgRoute.None, 0, null);

            using var memory = new MemoryStream(data);
            using var reader = new BinaryReader(memory);
            byte pktMask = reader.ReadByte();
            ushort msgType = reader.ReadUInt16();
            MsgRoute route = (MsgRoute)reader.ReadByte();
            ulong targetSession = reader.ReadUInt64();
            int len = reader.ReadInt32();
            byte[] body = reader.ReadBytes(len);
            if (pktMask != PKT_HEAD_BYTE)
            {
                Debug.Log($"<color=green>协议头错误！！直接关闭连接</color>");
                return (false, null, msgType, route, targetSession, body);
            }
            byte[] remain = null;
            int rem = data.Length - len - HEAD_LEN;
            if (rem > 0)
            {
                remain = new byte[rem];
                Buffer.BlockCopy(data, len + HEAD_LEN, remain, 0, rem);
            }
            return (true, remain, msgType, route, targetSession, body);
        }
    }
}
