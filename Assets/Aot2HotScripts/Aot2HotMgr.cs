#define USE_TMP_FONT
using UnityEngine;
using TMPro;
using YooAsset;
using Aot;
using System.Collections;
using UnityEngine.UI;
using System;
using Object = UnityEngine.Object;
using HybridCLR;
using System.Reflection;
using FairyGUI;

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
            StartCoroutine(nameof(IEAwake));
#if USE_GM
            var report = GameObject.FindObjectOfType<Reporter>(true);
            if (report != null) {
                report.gameObject.SetActive(true);
            }
#endif
        }
        IEnumerator IEAwake()
        {
            var assets = YooAssets.LoadAllAssetsAsync<Object>("Assets/Res/Aot2Hot/Font/JTFont.ttf");
            yield return assets;
#if USE_TMP_FONT
            TMP_FontAsset font = null;
#else
            Font font = null;
#endif
            foreach (var ass in assets.AllAssetObjects) {
#if USE_TMP_FONT
                if (ass is TMP_FontAsset f)
#else
                if (ass is Font f)
#endif
                {
                    font = f;
                    break;
                }
            }
#if USE_TMP_FONT
            FontManager.RegisterFont(new TMPFont() { fontAsset = font, name = "JTFont" });//tmp pro 字体 有点糊
#else
            FontManager.RegisterFont(new DynamicFont("JTFont", font), "JTFont");
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
                StartCoroutine(nameof(DownloadHotRes));
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
            StartCoroutine(nameof(DownloadHotRes));
        }
        void DownLoadEnd()
        {
            StartCoroutine(nameof(GotoHotScene));
        }
        IEnumerator GotoHotScene()
        {
            progerssImg.fillAmount = 1;

            //// 注意：location只需要填写资源包里的任意资源地址。
            var rawAotOp = YooAssets.LoadAllAssetsAsync<TextAsset>("Assets/Res/Raw/Aot/mscorlib.bytes");
            yield return rawAotOp;

            // Editor下无需加载，直接查找获得HotUpdate程序集
            foreach (var asset in rawAotOp.AllAssetObjects) {
#if !UNITY_EDITOR
                var err = RuntimeApi.LoadMetadataForAOTAssembly(((TextAsset)asset).bytes, HomologousImageMode.SuperSet);
                Debug.Log($"LoadMetadataForAOTAssembly:{asset.name}. ret:{err}");
#endif
            }
            rawAotOp.Release();

            var rawHotOp = YooAssets.LoadAllAssetsAsync<TextAsset>("Assets/Res/Raw/Hot/Hot.bytes");
            yield return rawHotOp;
            foreach (var asset in rawHotOp.AllAssetObjects) {
#if !UNITY_EDITOR
                var ass = Assembly.Load(XIHDecryptionServices.Decrypt(((TextAsset)asset).bytes));
#endif
            }
            rawHotOp.Release();

            //Debug.Log($"成功加载热更程序集和元数据");
            var handle= YooAssets.LoadSceneAsync("Assets/Res/HotScene/HotInit.unity");
        }
    }
}
