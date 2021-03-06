using LitJson;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace XIHBasic
{
    public class ResCfgWindow : EditorWindow
    {
        [MenuItem("XIHUtil/ResCfgWindow")]
        static void OpenUrlConfigWindow()
        {
            EditorWindow.GetWindow(typeof(ResCfgWindow), true, "ResCfgWindow", true);
        }
        private void OnEnable()
        {
            mainUrl = "http://127.0.0.1:5000/";
            dllVersion = "1.0.0";
            connKey = "Assets/Bundles/CheckAANetConn.txt";
            connBundleName = "aecd6b06c08b86e6e367ab1f201c5120";
            loginIp = "127.0.0.1";
            loginPort = 12345;
            loginKcp = true;
            if (File.Exists(edtCfgPath))
            {
                var data = JsonMapper.ToObject(File.ReadAllText(edtCfgPath));
                mainUrl = (string)(data["mainUrl"] ?? dllVersion);
                dllVersion = (string)(data["dllVersion"] ?? dllVersion);
                connKey = (string)(data["key"] ?? connKey);
                connBundleName = (string)(data["bundleName"] ?? connBundleName);
                loginIp = (string)(data["loginIp"] ?? loginIp);
                loginPort = (int)(data["loginPort"] ?? loginPort);
                loginKcp = (bool)(data["isKcp"] ?? loginKcp);
            }
        }
        string mainUrl;//资源下载地址
        string dllVersion;//当前Dll版本
        string connKey;//判断AA网络是否正常所尝试的Key
        string connBundleName;//connKey所在bundle的名字
        string loginIp;
        int loginPort;
        bool loginKcp;
        readonly string resCfgPath = $"Assets/Resources/{PlatformConfig.CONFIG_NAME}.json";//配置文件目录
        readonly string resDllPath = $"Assets/Resources/{PlatformConfig.HOTFIX_DLL_NAME}.bytes";//DLL配置文件目录
        readonly string edtCfgPath = $"Assets/Editor/XIHEditor/{PlatformConfig.CONFIG_NAME}_svr.json";//服务端配置文件目录

        const string webResOutPath= "XIHServer/Res/WebBin/Game/";//dll和资源输出路径
        readonly string dllPath = $"Library/ScriptAssemblies/{PlatformConfig.HOTFIX_DLL_NAME}.dll";
        readonly string pdbPath = $"Library/ScriptAssemblies/{PlatformConfig.HOTFIX_DLL_NAME}.pdb";

        void OnGUI()
        {
            //设置整个界面是以垂直方向来布局
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();

            mainUrl = EditorGUILayout.TextField("Main Url", mainUrl);
            dllVersion = EditorGUILayout.TextField("Dll Version", dllVersion);
            connKey = EditorGUILayout.TextField("检测AA网络尝试的Key", connKey);
            connBundleName = EditorGUILayout.TextField("Key所在bundle的名字", connBundleName);
            loginIp = EditorGUILayout.TextField("LoginIp", loginIp);
            loginPort = EditorGUILayout.IntField("LoginPort", loginPort);
            loginKcp = EditorGUILayout.Toggle("Is Kcp,Otherwise Tcp", loginKcp);
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox($"Key只是为了检测AA网络顺畅，因为UpdateCatalogs不管网络是否连接都会执行成功，导致无网络无法更新hash和json，就默认使用缓存，造成catalog.json以为是最新\r\n查看远程资源Bundle的信息前最好在编辑器运行时进行,AA服务器开启状态且已更新最新的hash和catalog.json，运行模式选择`Play Mode Script>Use Existing Build`,避免漫长的等待\r\n默认Key: Assets/Bundles/CheckAANetConn.txt\r\nbundleName: aecd6b06c08b86e6e367ab1f201c5120\r\n因为只是为了检测AA网络，所以该group最好不要再添加任何资源，保持bundle内存最小", MessageType.Warning);
            if (GUILayout.Button("查看远程资源Bundle的信息,显示全部远程bundle名字"))
            {
                LogRemoteBundleName();
            }
            EditorGUILayout.HelpBox($"当前配置文件输出目录{resCfgPath}\r\n当前Dll和配置文件输出目录:{webResOutPath}", MessageType.Info);
            EditorGUILayout.Space();
            if (GUILayout.Button("输出Dll和配置文件到目标目录"))
            {
                if (string.IsNullOrEmpty(webResOutPath)) {
                    Debug.LogError("路径不能为空");
                }
                if (!Directory.Exists(webResOutPath)) {
                    Directory.CreateDirectory(webResOutPath);
                }
                if (EditorUtility.DisplayDialog("打包确认弹框", $"即将Dll和配置文件到目标目录:{Path.GetFullPath(webResOutPath)}", "确定", "取消"))
                {
                    //先输出Dll到目标路径
                    string urlDllPath = $"{webResOutPath}/{PlatformConfig.HOTFIX_DLL_NAME}_{PlatformConfig.PLATFORM_NAME}";
                    File.Copy(dllPath, urlDllPath, true);

                    JsonData data = new JsonData
                    {
                        ["mainUrl"] = mainUrl ?? "",
                        ["dllVersion"] = dllVersion ?? ""
                    };
                    var json = JsonMapper.ToJson(data);
                    File.WriteAllText(resCfgPath, json);
                    data["key"] = connKey ?? "";
                    data["bundleName"] = connBundleName ?? "";
                    data["loginIp"] = loginIp ?? "127.0.0.1";
                    data["loginPort"] = loginPort;
                    data["isKcp"] = loginKcp;
                    json = JsonMapper.ToJson(data);
                    File.WriteAllText(edtCfgPath, json);
                    File.Copy(dllPath, resDllPath, true);

                    string urlCfgPath = $"{webResOutPath}/{PlatformConfig.CONFIG_NAME}_{PlatformConfig.PLATFORM_NAME}";
                    File.WriteAllText(urlCfgPath, json);
                    Debug.Log($"已生成配置：{resCfgPath}、{edtCfgPath}、{resDllPath}; \r\n已复制{Path.GetFileName(urlDllPath)},{Path.GetFileName(urlCfgPath)} 到目录:{Path.GetFullPath(webResOutPath)}");
                    string persistentPath = PlatformConfig.PersistentDataPath;
                    if (File.Exists($"{persistentPath}/{PlatformConfig.CONFIG_NAME}"))
                    {
                        File.Delete($"{persistentPath}/{PlatformConfig.CONFIG_NAME}");
                    }
                    if (File.Exists($"{persistentPath}/{PlatformConfig.HOTFIX_DLL_NAME}"))
                    {
                        File.Delete($"{persistentPath}/{PlatformConfig.HOTFIX_DLL_NAME}");
                    }
                    File.Copy(pdbPath, $"{persistentPath}/{Path.GetFileName(pdbPath)}", true);
                    AssetDatabase.Refresh();
                }
            }
            EditorGUILayout.EndVertical();
        }
        async void LogRemoteBundleName() {
            await Addressables.InitializeAsync().Task;
            await Addressables.UpdateCatalogs().Task;
            foreach (var k in Addressables.ResourceLocators)
            {
                var locs = await Addressables.LoadResourceLocationsAsync(k.Keys, Addressables.MergeMode.Union).Task;
                foreach (var location in locs)
                {
                    var id = location.InternalId;
                    if (id != null && id.StartsWith("http"))
                    {
                        var bif = location.Data as AssetBundleRequestOptions;
                        var locHash = Hash128.Parse(bif.Hash);
                        Debug.Log($"InternalId: {id}\r\nBundleName: {bif.BundleName}\r\nBundleHash: {locHash}\r\n");
                    }
                }
            }
        }
    }
}
