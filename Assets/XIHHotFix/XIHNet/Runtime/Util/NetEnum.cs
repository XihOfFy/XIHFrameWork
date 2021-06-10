using ProtoBuf;
using System;

namespace XiHNet
{
    [Flags]
    public enum NetServer
    {
        All = -1,//服务端表示开启全部服务器，可以在不同端口；客户端表示开启一个Socket，且包含所有通信功能（需要服务器全部端口和协议一致才行，即只开启一个服务器，包含全部功能）
        Login = 1,
        Lobby=1<<1,
        Battle=1<<2,
    }
    //[ProtoContract]ILRuntime下Pb不支持枚举
    public enum NetworkProtocol
    {
        Kcp = 1,
        Tcp = 2,
    }
    //[ProtoContract]
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
