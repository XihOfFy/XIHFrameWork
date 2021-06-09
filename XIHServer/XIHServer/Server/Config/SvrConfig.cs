using System.Collections.Generic;
using System.Net;
using XiHNet;

namespace XIHServer
{
    public class SvrConfig
    {
        //public string Addr;
        //public int Port;
        public IPEndPoint IPEndPoint { get; }
        public NetServer SvrType { get; }
        public NetworkProtocol NetProtocol { get; }
        public CryptType CryptType { get; }
        public SvrConfig(IPEndPoint iPEndPoint, NetServer svrType, NetworkProtocol netProtocol, CryptType cryptType)
        {
            IPEndPoint = iPEndPoint;
            SvrType = svrType;
            NetProtocol = netProtocol;
            CryptType = cryptType;
        }
    }
}
