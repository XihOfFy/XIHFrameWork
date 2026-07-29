using System;

namespace XiHNet
{
    public enum NetworkProtocol
    {
        Kcp = 1,
        Tcp,
        WXTCP,
        WXUDP,
    }
    public enum CryptType
    {
        CryptNone = 0,
        CryptXor = 1,
        CryptAes = 2,
    }
    public enum NetState
    {
        Open = 1,
        Closed = 3
    }
    /// <summary>
    /// 消息路由标志（可组合）
    /// <para><see cref="Broadcast"/>: 广播给其他所有客户端（不含发送者）</para>
    /// <para><see cref="ToHost"/>: 通知主机的本地客户端</para>
    /// <para><see cref="ToSession"/>: 指定发送给某个客户端（需设置 <see cref="IMessage.TargetSession"/>）</para>
    /// </summary>
    [Flags]
    public enum MsgRoute : byte
    {
        None = 0,
        Broadcast = 1,
        ToHost = 1 << 1,
        ToSession = 1 << 2,
    }
}
