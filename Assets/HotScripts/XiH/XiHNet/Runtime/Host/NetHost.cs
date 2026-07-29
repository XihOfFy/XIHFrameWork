using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using XIHServer;

namespace XiHNet
{
    /// <summary>
    /// 联机服务容器：
    /// - Host 模式：本机运行 Server + 一个本地 Client（LocalClient），外部玩家可连入该 Server。
    /// - 纯 Server 模式（dedicated）：只跑 Server 不建本地 Client，作为专用房间。
    /// 通过 <see cref="CreateHost"/> / <see cref="CreateServerOnly"/> 两个工厂方法区分构造，
    /// 其余生命周期（Start/Close）一致，GUI 层按需判断。
    /// </summary>
    public class NetHost
    {
        public NetAdapter LocalClient { get; private set; }
        public bool IsRunning { get; private set; }
        /// <summary>是否仅运行服务端（不创建 LocalClient）</summary>
        public bool IsServerOnly { get; private set; }

        public NetworkProtocol Protocol { get; }
        public int Port { get; }

        private readonly object server;

        private NetHost(NetworkProtocol protocol, int port, CryptType crypt, bool serverOnly)
        {
            IMessageExt.Init();

            Protocol = protocol;
            Port = port;
            IsServerOnly = serverOnly;

            var serverEndPoint = new IPEndPoint(IPAddress.Any, port);
            switch (protocol)
            {
#if !UNITY_WEBGL || UNITY_EDITOR
                case NetworkProtocol.Kcp:
                    server = new KcpServer(serverEndPoint, crypt);
                    break;
                case NetworkProtocol.Tcp:
                    server = new TcpServer(serverEndPoint, crypt);
                    break;
#endif
                default:
                    throw new NotSupportedException($"Host mode does not support {protocol}");
            }

            if (!serverOnly)
            {
                var clientEndPoint = new IPEndPoint(IPAddress.Loopback, port);
                LocalClient = new NetAdapter(protocol, clientEndPoint);
            }
            Debug.Log($"[NetHost] Server started on port {port} ({protocol}, {crypt}) serverOnly={serverOnly}");
        }

        public static NetHost CreateHost(NetworkProtocol protocol, int port, CryptType crypt = CryptType.CryptNone)
            => new NetHost(protocol, port, crypt, serverOnly: false);

        public static NetHost CreateServerOnly(NetworkProtocol protocol, int port, CryptType crypt = CryptType.CryptNone)
            => new NetHost(protocol, port, crypt, serverOnly: true);

        public void Start()
        {
            if (IsRunning) return;
            IsRunning = true;
            LocalClient?.Connect();
        }

        /// <summary>获取当前服务端上已接入的会话（Host 模式含本机 LocalClient）；未运行时返回空列表。</summary>
        public List<NetPeerSnapshot> GetConnectedPeersSnapshot()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            switch (server)
            {
                case KcpServer kcp:
                    return kcp.GetConnectedPeersSnapshot();
                case TcpServer tcp:
                    return tcp.GetConnectedPeersSnapshot();
            }
#endif
            return new List<NetPeerSnapshot>();
        }

        public void Close()
        {
            if (!IsRunning) return;
            IsRunning = false;
            LocalClient?.Close();
            LocalClient = null;
            switch (server)
            {
#if !UNITY_WEBGL || UNITY_EDITOR
                case KcpServer kcp: kcp.Close(); break;
                case TcpServer tcp: tcp.Close(); break;
#endif
            }
            Debug.Log("[NetHost] Host closed");
        }
    }

    /// <summary>
    /// 服务端当前已接入会话的快照（用于调试 UI）。
    /// </summary>
    public readonly struct NetPeerSnapshot
    {
        public ulong SessionKey { get; }
        /// <summary>远端地址，如 192.168.1.2:12345</summary>
        public string RemoteEndpoint { get; }
        /// <summary>是否为 Host 模式下本机 LocalClient 对应的会话（首个接入）。</summary>
        public bool IsHostLocalAdapter { get; }

        public NetPeerSnapshot(ulong sessionKey, string remoteEndpoint, bool isHostLocalAdapter)
        {
            SessionKey = sessionKey;
            RemoteEndpoint = remoteEndpoint ?? "";
            IsHostLocalAdapter = isHostLocalAdapter;
        }
    }
}
