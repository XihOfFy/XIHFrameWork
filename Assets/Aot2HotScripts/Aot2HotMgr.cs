#define USE_TMP_FONT
using UnityEngine;
using TMPro;
using Aot;
using System.Collections;
using UnityEngine.UI;
using System;
using Object = UnityEngine.Object;
using HybridCLR;
using System.Reflection;
using FairyGUI;
using YooAsset;
using Cysharp.Threading.Tasks;

namespace Aot2Hot
{
    //这个类的方法都改为携程，等全部下载完，补充元数据后再考虑使用unitask
    public partial class Aot2HotMgr : MonoBehaviour
    {
        public TMP_Text tip;
        public UnityEngine.UI.Image progerssImg;
        private void Awake()
        {
            tip.text = "请稍等一会";
            progerssImg.fillAmount = 0;
            StartCoroutine(IEAwake());
#if USE_GM
            var report = GameObject.FindObjectOfType<Reporter>(true);
            if (report != null) {
                report.gameObject.SetActive(true);
            }
#endif
        }
        IEnumerator IEAwake()
        {
#if USE_TMP_FONT
            var assets = AssetLoadUtil.LoadAssetAsync<TMP_FontAsset>("Assets/Res/Aot2Hot/Font/JTFontTMP.asset");
#else
            var assets = AssetLoadUtil.LoadAssetAsync<Font>("Assets/Res/Aot2Hot/Font/JTFont.ttf");
#endif
            yield return assets;
#if USE_TMP_FONT
            FontManager.RegisterFont(new TMPFont() { fontAsset = assets.GetAsset<TMP_FontAsset>(), name = "JTFont" });//tmp pro 字体 有点糊
#else
            FontManager.RegisterFont(new DynamicFont("JTFont", assets.GetAsset<Font>()), "JTFont");
#endif
            UIConfig.defaultFont = "JTFont"; //另一个方法是FGUI项目里添加jtfont字体，直接引用，发布时字体不会发布，而是找该字体的注册 https://www.fairygui.com/docs/editor/font
            UIPackage.unloadBundleByFGUI = false;

            var localVer = new Version(Application.version);
            var remteVer = new Version(AotConfig.frontConfig.focusVersion);
            if (remteVer.CompareTo(localVer) > 0)
            {
                tip.text = "当前本版过低，请更新后再启动游戏...";
            }
            else { 
                StartCoroutine(DownloadHotRes());
            }
        }
        //string[] suffixArr = new string[] { "._. . . . .", ". ._. . . .", ". . ._. . .", ". . . ._. .",  ". . . . ._." };
        void OnDownloadProgress(DownloadUpdateData updateData)
        {
            tip.text = $"下载资源中。。。({updateData.CurrentDownloadCount}/{updateData.TotalDownloadCount}): {(updateData.CurrentDownloadCount >> 10)}KB/{(updateData.TotalDownloadCount >> 10)}KB";
            //tip.text = $"下载资源中 {suffixArr[currentDownloadCount%6]}";
            if (updateData.TotalDownloadCount > 0)
            {
                progerssImg.fillAmount = 1.0f * updateData.CurrentDownloadCount / updateData.TotalDownloadBytes;
            }
        }
        void TryReDownload() {
            StartCoroutine(DownloadHotRes());
        }
        void DownLoadEnd()
        {
            GotoHotScene().Forget();
        }
        async UniTaskVoid GotoHotScene()
        {
            progerssImg.fillAmount = 1;

            //// 注意：location只需要填写资源包里的任意资源地址。
            var rawAotOp = AssetLoadUtil.LoadAllAssetsAsync<TextAsset>("Assets/Res/Raw/Aot/mscorlib.bytes");
            //yield return rawAotOp;
            await rawAotOp.ToUniTask();
            // Editor下无需加载，直接查找获得HotUpdate程序集
            var ass = rawAotOp.GetAssets<TextAsset>();
            foreach (var asset in ass) {
#if !UNITY_EDITOR
                var err = RuntimeApi.LoadMetadataForAOTAssembly((asset).bytes, HomologousImageMode.SuperSet);
                Debug.Log($"LoadMetadataForAOTAssembly:{asset.name}. ret:{err}");
#endif
            }
            rawAotOp.Release();

            var rawHotOp = AssetLoadUtil.LoadAllAssetsAsync<TextAsset>("Assets/Res/Raw/Hot/Hot.bytes");
            //yield return rawHotOp;
            await rawHotOp.ToUniTask();
            ass = rawHotOp.GetAssets<TextAsset>();
            foreach (var asset in ass) {
#if !UNITY_EDITOR
                var ass = Assembly.Load(XIHDecryptionServices.Decrypt((asset).bytes));
#endif
            }
            rawHotOp.Release();

            //Debug.Log($"成功加载热更程序集和元数据");
            AssetLoadUtil.LoadScene("Assets/Res/HotScene/HotInit.unity").Forget();
        }
    }
}
