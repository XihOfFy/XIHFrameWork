using System;
using System.IO;
using UnityEngine;

namespace Aot
{
    public class AotConfig
    {
        public const string PACKAGE_NAME = "DefaultPackage";
        public static FrontConfig frontConfig = new FrontConfig();
        public static string GetFrontUrl() {
            string url = "https://gitee.com/xihoffy/PublicAccess/raw/master/Front/";
            //string url = "http://localhost:5000/Front/";
#if UNITY_EDITOR
            //url = $"http://{GetIP()}:5000/Front/";
            url += $"{UnityEditor.EditorUserBuildSettings.activeBuildTarget}.json";
#else
            if (Application.platform == RuntimePlatform.Android)
                url += "Android.json";
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
                url += "iOS.json";
            else if (Application.platform == RuntimePlatform.WebGLPlayer)
                url += "WebGL.json";
            else
                url += "StandaloneWindows64.json";
#endif
            return url;
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        public static void InitFrontConfig()
        {
            var dir = "XIHWebServerRes/Front";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var preffixUrl = "https://gitee.com/xihoffy/PublicAccess/raw/master/";//自己指定ip
            var suffix = "/StreamingAssets/yoo/DefaultPackage";
            //string preffixUrl = $"http://localhost:5000/";//自动获取本机的ip
            var config = new FrontConfig();
            config.focusVersion = "0.0.0";

            config.defaultHostServer = preffixUrl+"Android"+ suffix;
            config.fallbackHostServer = preffixUrl + "Android" + suffix;
            var file = $"{dir}/Android.json";
            if (!File.Exists(file))
            {
                File.WriteAllText(file, JsonUtility.ToJson(config));
            }

            config.defaultHostServer = preffixUrl + "iOS" + suffix;
            config.fallbackHostServer = preffixUrl + "iPhone" + suffix;
            file = $"{dir}/iOS.json";
            if (!File.Exists(file))
            {
                File.WriteAllText(file, JsonUtility.ToJson(config));
            }

            config.defaultHostServer = preffixUrl + "WebGL" + suffix;
            config.fallbackHostServer = preffixUrl + "WebGL" + suffix;
            file = $"{dir}/WebGL.json";
            if (!File.Exists(file))
            {
                File.WriteAllText(file, JsonUtility.ToJson(config));
            }

            config.defaultHostServer = preffixUrl + "StandaloneWindows64" + suffix;
            config.fallbackHostServer = preffixUrl + "StandaloneWindows64" + suffix;
            file = $"{dir}/StandaloneWindows64.json";
            if (!File.Exists(file))
            {
                File.WriteAllText(file, JsonUtility.ToJson(config));
            }
        }

        public static string GetIP() {
            string hostname = System.Net.Dns.GetHostName();
            var ipadrlist = System.Net.Dns.GetHostEntry(hostname);
            var localaddrs = ipadrlist.AddressList;
            foreach (var add in localaddrs)
            {
                if (add.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !add.ToString().EndsWith(".1")) return add.ToString();
            }
            return "127.0.0.1";
        }
#endif
    }
    [Serializable]
    public class FrontConfig
    {
        public string focusVersion="0.0.0";//最低强更版本
        //yooasset的下载资源路径，后期可以扩展其他的
        public string defaultHostServer;
        public string fallbackHostServer;
    }

}
