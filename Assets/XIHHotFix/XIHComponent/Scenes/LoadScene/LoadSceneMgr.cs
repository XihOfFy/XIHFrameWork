using LitJson;
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
    public class LoadSceneMgr : AbsComponent<MonoDotBase>
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

        private string newMainUrl = "";
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
            string json;
            if (File.Exists(PathConfig.ConfigPath))
            {
                json = File.ReadAllText(PathConfig.ConfigPath);
            }
            else
            {//HotFixInit.LoadScene()加载此场景超时但却加载成功会出现此问题，会删除外置存储的配置文件；这里保守起见使用Resources加载
                json = Resources.Load<TextAsset>(PlatformConfig.CONFIG_NAME).text;
            }
            var data = JsonMapper.ToObject(json);
            string mainUrl = data["mainUrl"].ToString();
            if (!mainUrl.EndsWith("/")) mainUrl += "/";
            string dllVersion = data["dllVersion"].ToString();
            version.text = dllVersion;
            ShowTips("检测更新", "是否检测更新？", () => CheckDllUpdate(mainUrl, dllVersion), () => Application.Quit());//至少一次使用Action传递()=> Application.Quit()而不是Application.Quit，不然CLR无法自动分析出Quit方法需要绑定注册
            Debug.Log("OnEnable");
        }
        protected override void OnDisable()
        {
            HotFixInit.Update -= Update;
            HotFixInit.FixedUpdate -= FixedUpdate;
            Debug.Log("OnDisable");
        }
        readonly float barSpeed = 8 * Time.fixedDeltaTime;
        void Update()
        {
            if (progress >= 1.0f)
            {
                precentText.text = "等待进入游戏";
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
        private async void CheckDllUpdate(string prixUrl, string dllVersion)
        {
            try
            {
                //若手机无法发送Http，修改为Https
                HttpWebRequest request = HttpWebRequest.CreateHttp($"{prixUrl}{PlatformConfig.CONFIG_NAME}_{PlatformConfig.PLATFORM_NAME}");
                using (WebResponse wr = await request.GetResponseAsync())
                {
                    using (StreamReader sr = new StreamReader(wr.GetResponseStream()))
                    {
                        string json = sr.ReadToEnd();
                        var data = JsonMapper.ToObject(json);
                        string newDllVer = data["dllVersion"].ToString();
                        newMainUrl = data["mainUrl"].ToString();
                        string bundleName = data["bundleName"].ToString();
                        string key = data["key"].ToString();
                        IPConfig.CurCfg.loginIp = data["loginIp"].ToString();
                        IPConfig.CurCfg.loginPort = (int)data["loginPort"];
                        IPConfig.CurCfg.netType = (bool)data["isKcp"] ? NetworkProtocol.Kcp : NetworkProtocol.Tcp;
                        if (newDllVer == dllVersion)
                        {
                            CheckResUpdate(bundleName, key);
                        }
                        else
                        {
                            DownLoadNewDll($"{prixUrl}{PlatformConfig.HOTFIX_DLL_NAME}_{PlatformConfig.PLATFORM_NAME}", () =>
                            {
                                data = new JsonData
                                {
                                    ["mainUrl"] = newMainUrl,
                                    ["dllVersion"] = newDllVer
                                };
                                File.WriteAllText(PathConfig.ConfigPath, JsonMapper.ToJson(data));//下载完毕才保存
                                CheckResUpdate(bundleName, key);
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"url:{prixUrl}{PlatformConfig.CONFIG_NAME},{e}");
                ShowTips("版本更新错误", "无法正常更新，请检验网络是否连接", () => CheckDllUpdate(prixUrl, dllVersion), Application.Quit);
            }
        }
        bool needQuit = false;
        private async void DownLoadNewDll(string dllUrl, Action completed)
        {
            needQuit = true;
            progress = 0.05f;
            try
            {
                await await Task.Factory.StartNew(async () =>
                {
                    HttpWebRequest request = HttpWebRequest.CreateHttp(dllUrl);
                    using (WebResponse wr = await request.GetResponseAsync())
                    {
                        using (BinaryReader br = new BinaryReader(wr.GetResponseStream()))
                        {
                            using (FileStream fs = new FileStream(PathConfig.DllPath, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                progress = 0.1f;
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
                });
                completed();//Keep In MainThread
            }
            catch (Exception e)
            {
                Debug.LogError($"url:{dllUrl},{e}");
                ShowTips("版本下载错误", "下载过程错误，请检验网络是否连接", () => DownLoadNewDll(dllUrl, completed), Application.Quit);
            }
        }
        private async Task<List<IResourceLocation>> ClearOldAndGetNewRes()
        {
            await Addressables.UpdateCatalogs().Task;//更新json:checking for catalog updates automatically
            string cachaPath = Caching.currentCacheForWriting.path;
            var dirs = Directory.GetDirectories(cachaPath);
            var list = new List<IResourceLocation>();
            HashSet<string> remainDirs = new HashSet<string>();
            foreach (var k in Addressables.ResourceLocators)
            {
                var locs = await Addressables.LoadResourceLocationsAsync(k.Keys, Addressables.MergeMode.Union).Task;
                foreach (var loc in locs)
                {
#if ENABLE_CACHING
                    var id = loc.InternalId;
                    if (id != null && id.StartsWith("http"))
                    {
                        var abOp = loc.Data as AssetBundleRequestOptions;
                        bool nd = true;
                        var locHash = Hash128.Parse(abOp.Hash);
                        if (locHash.isValid)
                        {
                            if (Caching.IsVersionCached(new CachedAssetBundle(abOp.BundleName, locHash)))
                            {
                                nd = false;
                                remainDirs.Add(Path.Combine(cachaPath, abOp.BundleName));
                                Caching.ClearOtherCachedVersions(abOp.BundleName, locHash);
                                Debug.Log($"newest> InternalId: {loc.InternalId}\r\n PrimaryKey: {loc.PrimaryKey}\r\n ProviderId: {loc.ProviderId}\r\n ResourceType: {loc.ResourceType}\r\n DependencyHashCode: {loc.DependencyHashCode}\r\n BundleName: {abOp.BundleName}\r\n Hash: {abOp.Hash}\r\n");
                            }
                        }
                        if (nd)
                        {
                            //为何这不使用Caching.ClearAllCachedVersions(abOp.BundleName)，因为会出现空文件夹，后面手动清理也会处理，就重复了
                            list.Add(loc);
                            Debug.LogWarning($"update> InternalId: {loc.InternalId}\r\n PrimaryKey: {loc.PrimaryKey}\r\n ProviderId: {loc.ProviderId}\r\n ResourceType: {loc.ResourceType}\r\n DependencyHashCode: {loc.DependencyHashCode}\r\n BundleName: {abOp.BundleName}\r\n Hash: {abOp.Hash}\r\n");
                        }
                    }
#else
                list.Add(loc);
#endif
                }
            }
            //手动清理多余资源，
            foreach (var dir in dirs)
            {
                if (remainDirs.Contains(dir))
                {
                    Debug.Log($"remaining Dir:{dir}");
                }
                else
                {
                    Debug.LogWarning($"delete Dir:{dir}");
                    Directory.Delete(dir, true);
                }
            }
            return list;
        }
        private async void CheckResUpdate(string bundleName, string key)
        {
            Caching.ClearAllCachedVersions(bundleName);
            var connHandler = Addressables.LoadAssetAsync<TextAsset>(key);
            await connHandler.Task;
            bool isOK = connHandler.Status == AsyncOperationStatus.Succeeded;
            Addressables.Release(connHandler);
            if (isOK)
            {
                //上面操作只是为了确保AA网络顺畅，因为UpdateCatalogs不管网络是否连接都会执行成功，导致无网络无法更新hash和json，就默认使用缓存，造成catalog.json以为是最新
                //var irls = await DelUnusedAndGetNewRes();
                var irls = await ClearOldAndGetNewRes();
                if (irls.Count > 0)
                {
                    DownLoadNewResOnce(irls);
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
        private async void Skip()
        {
            if (needQuit)
            {
                progress = 0.99f;
                ShowTips("版本更新", "版本更新完毕，请重新运行,体验最新内容", Application.Quit, Application.Quit);
            }
            else
            {
                progress = 1.0f;
                var handle = Addressables.LoadSceneAsync(PathConfig.AA_Scene_Login).Task;
                bool pass = false;
                async void DoWait()
                {
                    await Task.Delay(5000);
                    if (pass) return;
                    if (handle.Status != TaskStatus.RanToCompletion)
                    {
                        //此处报错一般是因为热更资源引用了新的Unity本地资源(需要替换新apk)
                        //所以为了避免该情况，热更资源应该尽可能只使用热更资源所引用的资源；或设计时多引用本地资源（可能此时用不到，但以后热更可能会引用到）
                        PathConfig.ClearAll();
                        ShowTips("版本不兼容", $"请在官网[{newMainUrl}]下载最新Apk", () => Application.OpenURL(newMainUrl), Application.Quit);
                    }
                }
                DoWait();
                await handle;//这个报错是无法捕获异常的，因为属于其他异步Task中,且报错后此await处于漫长等待...若报错则不可能加载成功，属于替换apk才能解决的情况
                pass = true;
            }
        }
    }
}
