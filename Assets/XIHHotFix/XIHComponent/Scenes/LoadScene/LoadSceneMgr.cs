using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UI;
using XIHBasic;
using XiHNet;

namespace XIHHotFix
{
    public class LoadSceneMgr : AbsComponent
    {
        protected LoadSceneMgr(MonoDotBase dot) : base(dot) { }
        private float progress;
        private Text precentText;
        private Scrollbar bar;

        private GameObject tipsUI;
        private Text title;
        private Text content;
        private Text version;
        private Button cancel;
        private Button confirm;

        protected override void Awake()
        {
            progress = 0;
            precentText = MonoDot.GameObjsDic["Progress"].GetComponent<Text>();
            bar = MonoDot.GameObjsDic["Scrollbar"].GetComponent<Scrollbar>();
            tipsUI = MonoDot.GameObjsDic["Tips"];
            title = MonoDot.GameObjsDic["Title"].GetComponent<Text>();
            content = MonoDot.GameObjsDic["Ctt"].GetComponent<Text>();
            version = MonoDot.GameObjsDic["Ver"].GetComponent<Text>();
            cancel = MonoDot.GameObjsDic["Cancel"].GetComponent<Button>();
            confirm = MonoDot.GameObjsDic["Confirm"].GetComponent<Button>();
            Debug.Log("Awake");
        }
        protected override void OnEnable()
        {
            HotFixInit.Update += Update;
            HotFixInit.FixedUpdate += FixedUpdate;
            string configPath = $"{PlatformConfig.PersistentDataPath}/{PlatformConfig.CONFIG_NAME}";
            var data = LitJson.JsonMapper.ToObject(File.ReadAllText(configPath));
            string mainUrl = data["mainUrl"].ToString();
            if (!mainUrl.EndsWith("/")) mainUrl += "/";
            string dllVersion = data["dllVersion"].ToString();
            version.text = dllVersion;
            ShowTips("检测更新", "是否检测更新？", () => CheckDllUpdate(mainUrl, dllVersion, configPath), () => Application.Quit());//至少一次使用Action传递()=> Application.Quit()而不是Application.Quit，不然CLR无法自动分析出Quit方法需要绑定注册
            Debug.Log("OnEnable");
        }
        protected override void OnDisable()
        {
            HotFixInit.Update -= Update;
            HotFixInit.FixedUpdate -= FixedUpdate;
            Debug.Log("OnDisable");
        }
        bool isLoadingScene = false;
        readonly float barSpeed = 8 * Time.fixedDeltaTime;
        async void Update()
        {
            if (isLoadingScene) return;
            if (progress >= 1.0f)
            {
                precentText.text = "等待进入游戏";
                if (bar.size > 0.99f)
                {
                    isLoadingScene = true;
                    await Addressables.LoadSceneAsync("Assets/Bundles/Scenes/LoginScene.unity").Task;
                }
            }
        }
        void FixedUpdate()
        {
            bar.size = Mathf.SmoothStep(bar.size, progress, barSpeed);
            if (progress < 0.98f)
            {
                precentText.text = $"{bar.size * 100: 00.00}%";
            }
        }
        protected override void OnDestory()
        {
            Debug.Log("OnDestory");
        }
        private void ShowTips(string titleS, string contentS, Action confirmAct, Action cancelAct)
        {
            title.text = titleS;
            content.text = contentS;
            confirm.onClick.AddListener(() =>
            {
                confirm.onClick.RemoveAllListeners();
                tipsUI.SetActive(false);
                confirmAct();
            });
            cancel.onClick.AddListener(() =>
            {
                cancel.onClick.RemoveAllListeners();
                tipsUI.SetActive(false);
                cancelAct();
            });
            tipsUI.SetActive(true);
        }
        private async void CheckDllUpdate(string prixUrl, string dllVersion, string configPath)
        {
            try
            {
                //若手机无法发送Http，修改为Https
                HttpWebRequest request = HttpWebRequest.CreateHttp($"{prixUrl}{PlatformConfig.CONFIG_NAME}");
                using (WebResponse wr = await request.GetResponseAsync())
                {
                    using (StreamReader sr = new StreamReader(wr.GetResponseStream()))
                    {
                        string json = sr.ReadToEnd();
                        var data = LitJson.JsonMapper.ToObject(json);
                        string newDllVer = data["dllVersion"].ToString();
                        string newMainUrl = data["mainUrl"].ToString();
                        IPConfig.CurCfg.loginIp= data["loginIp"].ToString();
                        IPConfig.CurCfg.loginPort= (int)data["loginPort"];
                        IPConfig.CurCfg.netType = (bool)data["isKcp"]? NetworkProtocol.Kcp: NetworkProtocol.Tcp;
                        if (newDllVer == dllVersion)
                        {
                            CheckResUpdate(data["bundleName"].ToString(), data["key"].ToString());
                        }
                        else
                        {
                            DownLoadNewDll($"{prixUrl}{PlatformConfig.HOTFIX_DLL_NAME}_{PlatformConfig.PLATFORM_NAME}", configPath, json, data["bundleName"].ToString(), data["key"].ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"url:{prixUrl}{PlatformConfig.CONFIG_NAME},{e}");
                ShowTips("版本更新错误", "无法正常更新，请检验网络是否连接", () => CheckDllUpdate(prixUrl, dllVersion, configPath), Application.Quit);
            }
        }
        bool needQuit = false;
        private async void DownLoadNewDll(string dllUrl, string configPath, string json, string bundleName, string key)
        {
            needQuit = true;
            progress = 0.05f;
            try
            {
                await Task.Factory.StartNew(async () =>
                {
                    string dllPath = $"{PlatformConfig.PersistentDataPath}/{PlatformConfig.HOTFIX_DLL_NAME}";
                    HttpWebRequest request = HttpWebRequest.CreateHttp(dllUrl);
                    using (WebResponse wr = await request.GetResponseAsync())
                    {
                        using (BinaryReader br = new BinaryReader(wr.GetResponseStream()))
                        {
                            using (FileStream fs = new FileStream(dllPath, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                progress = 0.1f;//别设置1.0
                                int len = 1024 * 8;
                                byte[] chunk;
                                chunk = br.ReadBytes(len);
                                while (chunk.Length > 0)
                                {
                                    fs.Write(chunk, 0, chunk.Length);
                                    chunk = br.ReadBytes(len);
                                }
                            }
                        }
                    }
                    File.WriteAllText(configPath, json);//下载完毕才保存
                });
                CheckResUpdate(bundleName, key);
            }
            catch (Exception e)
            {
                Debug.LogError($"url:{dllUrl},{e}");
                ShowTips("版本下载错误", "下载过程错误，请检验网络是否连接", () => DownLoadNewDll(dllUrl, configPath, json, bundleName, key), Application.Quit);
            }
        }
        private async void CheckResUpdate(string bundleName, string key)
        {
            //Caching.ClearAllCachedVersions("aecd6b06c08b86e6e367ab1f201c5120");
            Caching.ClearAllCachedVersions(bundleName);
            // var connHandler = Addressables.LoadAssetAsync<TextAsset>("Assets/Bundles/CheckAANetConn.txt");
            var connHandler = Addressables.LoadAssetAsync<TextAsset>(key);
            await connHandler.Task;
            bool isOK = connHandler.Status == AsyncOperationStatus.Succeeded;
            Addressables.Release(connHandler);
            if (isOK)
            {
                //上面操作只是为了确保AA网络顺畅，因为UpdateCatalogs不管网络是否连接都会执行成功，导致无网络无法更新hash和json，就默认使用缓存，造成catalog.json以为是最新
                await Addressables.UpdateCatalogs().Task;//更新json:checking for catalog updates automatically
                var irls = new HashSet<IResourceLocation>();
                foreach (var k in Addressables.ResourceLocators)
                {
                    var locs = await Addressables.LoadResourceLocationsAsync(k.Keys, Addressables.MergeMode.Union).Task;
                    foreach (var vk in locs)
                    {
                        if (IsNeedDownload(vk))
                        {
                            irls.Add(vk);
                        }
                    }
                }
                if (irls.Count > 0)
                {
                    DownLoadNewResOnce(new List<IResourceLocation>(irls));
                }
                else
                {
                    Skip();
                }
            }
            else
            {
                ShowTips("资源更新", "资源无法检测更新，请检验网络是否连接", () => CheckResUpdate(bundleName, key), Application.Quit);
            }
        }
        //下载过程失败也不会重新下载已经下载好的
        async void DownLoadNewResOnce(IList<IResourceLocation> irls)
        {
            progress = 0f;
            var downloadHandler = Addressables.DownloadDependenciesAsync(irls, false);//如果释放句柄，您将无法通过“Status”属性检查操作句柄是否成功，因为释放会使操作句柄无效。
            GetProgress(downloadHandler);
            var res = await downloadHandler.Task;
            bool downSuccess = downloadHandler.Status == AsyncOperationStatus.Succeeded;
            Addressables.Release(downloadHandler);
            if (downSuccess)
            {
                Skip();
            }
            else
            {
                ShowTips("资源下载错误", "下载过程错误，请检验网络是否连接", () => DownLoadNewResOnce(irls), Application.Quit);
            }
        }
        async void GetProgress(AsyncOperationHandle downloadHandler)
        {
            while (progress < 0.98f && downloadHandler.IsValid())
            {
                progress = Mathf.Min(0.98f, downloadHandler.PercentComplete);
                await Task.Delay(250);
            }
        }
        public bool IsNeedDownload(IResourceLocation location)
        {
            var id = location.InternalId;
            if (id != null && id.StartsWith("http"))
            {
#if ENABLE_CACHING
                var bif = location.Data as AssetBundleRequestOptions;
                var locHash = Hash128.Parse(bif.Hash);
                bool nd = true;
                if (locHash.isValid)
                {
                    if (Caching.IsVersionCached(new CachedAssetBundle(bif.BundleName, locHash)))
                        nd = false;
                }
                if (nd)
                {
                    Caching.ClearAllCachedVersions(bif.BundleName);//这里删除缓存，新旧全部删除，因为新文件还没通过网络下载到缓存，所以删除也无妨
                    //Caching.ClearOtherCachedVersions(bif.BundleName, locHash);//删除其他缓存
                }
                Debug.Log($"{nd}:{id}:{bif.BundleName},{locHash}");
                return nd;
#endif //ENABLE_CACHING
            }
            return false;
        }
        public void Skip()
        {
            if (needQuit)
            {
                progress = 0.90f;
                ShowTips("版本更新", "版本更新完毕，请重新运行,体验最新内容", Application.Quit, Application.Quit);
                return;
            }
            progress = 1.0f;
            //Debug.Log($"6 :{string.Join("------------------------\r\n", Addressables.ResourceLocators.Select(s => string.Join("\r\n", s.Keys.Select(k => $"{k.GetType()}:{k}"))))}");
            Debug.Log("Skip");
        }
    }
}
