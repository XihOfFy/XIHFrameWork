using System;
using System.IO;
using UnityEngine;

namespace Aot
{
    public class AotConfig
    {
        public static string focusVersion="0.0.0";//最低强更版本
        public static FrontConfig frontConfig = new FrontConfig();
        public static string GetFrontUrl() {
            string url = "http://192.168.7.113:5000/Front/";
#if UNITY_EDITOR
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
            string preffixUrl = "http://192.168.7.113:5000/";
            var config = new FrontConfig();
            config.defaultHostServer = preffixUrl+"Android";
            config.fallbackHostServer = preffixUrl + "Android";
            var file = $"{dir}/Android.json";
            if (!File.Exists(file))
            {
                File.WriteAllText(file, JsonUtility.ToJson(config));
            }

            config.defaultHostServer = preffixUrl + "iOS";
            config.fallbackHostServer = preffixUrl + "iPhone";
            file = $"{dir}/iOS.json";
            if (!File.Exists(file))
            {
                File.WriteAllText(file, JsonUtility.ToJson(config));
            }

            config.defaultHostServer = preffixUrl + "WebGL";
            config.fallbackHostServer = preffixUrl + "WebGL";
            file = $"{dir}/WebGL.json";
            if (!File.Exists(file))
            {
                File.WriteAllText(file, JsonUtility.ToJson(config));
            }

            config.defaultHostServer = preffixUrl + "StandaloneWindows64";
            config.fallbackHostServer = preffixUrl + "StandaloneWindows64";
            file = $"{dir}/StandaloneWindows64.json";
            if (!File.Exists(file))
            {
                File.WriteAllText(file, JsonUtility.ToJson(config));
            }
        }
#endif
    }
    [Serializable]
    public class FrontConfig
    {
        //yooasset的下载资源路径，后期可以扩展其他的
        public string defaultHostServer;
        public string fallbackHostServer;
    }

}
