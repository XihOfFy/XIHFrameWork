using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XiHNet;

namespace XIHHotFix
{
    public sealed class IPConfig
    {
        public static IPConfig CurCfg { get; } = new IPConfig();
        private IPConfig() { }
        public string loginIp;
        public int loginPort;
        public NetworkProtocol netType;
    }
}
