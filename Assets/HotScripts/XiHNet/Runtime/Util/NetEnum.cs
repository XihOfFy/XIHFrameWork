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
}
