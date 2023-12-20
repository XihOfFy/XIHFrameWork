using System;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Aot
{
    public class AotConfig
    {
        public const string PACKAGE_NAME = "DefaultPackage";
        public static FrontConfig frontConfig = new FrontConfig();
        public static string GetFrontUrl() {
            string url = Resources.Load<XIHFrontSetting>(nameof(XIHFrontSetting)).front;
            if (!url.EndsWith("/")) url += "/";
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
            XIHFrontSetting frontSetting;
            var cfgPath = $"Assets/Resources/{nameof(XIHFrontSetting)}.asset";
            if (File.Exists(cfgPath)) {
                frontSetting = AssetDatabase.LoadAssetAtPath<XIHFrontSetting>(cfgPath);
            }
            else {
                frontSetting = ScriptableObject.CreateInstance<XIHFrontSetting>();
                frontSetting.front= $"http://{GetIP()}:5000/Front/";
                AssetDatabase.CreateAsset(frontSetting, cfgPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.LogError($"XIH:上线前记得配置{cfgPath}为公网路径");
            }
            var preffixUrl = frontSetting.front;//例如 http://127.0.0.1:5000/Front/
            if (preffixUrl.EndsWith("/")) preffixUrl = preffixUrl.Substring(0,preffixUrl.Length-1);//http://127.0.0.1:5000/Front
            preffixUrl = preffixUrl.Substring(0,preffixUrl.LastIndexOf('/')+1);//http://127.0.0.1:5000/

            var dir = "XIHWebServerRes/Front";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                Debug.LogError($"XIH:上线前记得配置{dir}里面的json，将下载地址改为公网地址");
            }
            var suffix = $"/StreamingAssets/yoo/{PACKAGE_NAME}";
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
