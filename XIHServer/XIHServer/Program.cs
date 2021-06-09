using LitJson;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using XiHNet;
namespace XIHServer
{
    class Program
    {
        public static Dictionary<NetServer, SvrConfig> SvrCfg { get; } = new Dictionary<NetServer, SvrConfig>();
        public static ConcurrentQueue<Action> ActQues { get; } = new ConcurrentQueue<Action>();
        static void Main(string[] args)
        {
            //Debugger.Log(string.Join(',',args));
            StartSvr();
            while (true)
            {
                try
                {
                    Thread.Sleep(1);
                    while (ActQues.TryDequeue(out Action act))
                    {
                        act();
                    }
                }
                catch (Exception e)
                {
                    Debugger.Log(e.ToString());
                }
            }
        }
        static void StartSvr()
        {
            JsonData config = JsonMapper.ToObject(File.ReadAllText("./Config/ServerCfg.json"));
            IPAddress iPAddress = IPAddress.Parse(config["Addr"].ToString());
            var tpi = int.Parse(config["SvrType"].ToString());
            foreach (JsonData data in config["Servers"])
            {
                int set = int.Parse(data["id"].ToString());
                if (((set & tpi)) == set)
                {
                    NetServer netTpye = (NetServer)set;
                    int port = int.Parse(data["port"].ToString());
                    NetworkProtocol protocol = (NetworkProtocol)int.Parse(data["netid"].ToString());
                    CryptType cryptType = (CryptType)int.Parse(data["cryptType"].ToString());
                    SvrCfg.Add(netTpye, new SvrConfig(new IPEndPoint(iPAddress, port), netTpye, protocol, cryptType));
                }
            }
            Dictionary<int, Dictionary<ushort, Action<AbsNetClient, byte[]>>> handleDics = new Dictionary<int, Dictionary<ushort, Action<AbsNetClient, byte[]>>>();
            static void BindPB(Dictionary<ushort, Action<AbsNetClient, byte[]>> handles, NetServer svrType)
            {
                switch (svrType)
                {
                    case NetServer.Login:
                        LoginHandle.BindHandle(handles);
                        break;
                    case NetServer.Lobby:
                        LobbyHandle.BindHandle(handles);
                        break;
                    case NetServer.Battle:
                        BattleHandle.BindHandle(handles);
                        break;
                }
            }
            foreach (var svr in SvrCfg)
            {
                int port = svr.Value.IPEndPoint.Port;
                if (handleDics.ContainsKey(port))
                {
                    Debugger.Log($"{svr.Value.SvrType}端口与其他服务端口共用，请确保该端口对应的服务包含此{svr.Value.SvrType}的功能，且协议类型都是 {svr.Value.NetProtocol} ，加密方式也一致");
                    BindPB(handleDics[port], svr.Key);
                    continue;
                }
                switch (svr.Value.NetProtocol)
                {
                    case NetworkProtocol.Kcp:
                        var kcp = new KcpServer(svr.Value.IPEndPoint, svr.Value.CryptType);//服务器在其他线程，防止公用线程阻塞
                        handleDics[port] = kcp.handles;
                        break;
                    case NetworkProtocol.Tcp:
                        var tcp = new TcpServer(svr.Value.IPEndPoint, svr.Value.CryptType);//服务器在其他线程，防止公用线程阻塞
                        handleDics[port] = tcp.handles;
                        break;
                }
                CommonHandle.BindHandle(handleDics[port]);
                BindPB(handleDics[port], svr.Key);
            }
            Debugger.Log("启动成功");
        }
    }
}
