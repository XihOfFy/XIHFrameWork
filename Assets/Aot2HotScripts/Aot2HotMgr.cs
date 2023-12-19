using UnityEngine;
using TMPro;
using System.Linq;
using YooAsset;
using HybridCLR;
using System.Reflection;
using Aot;
using System.Collections;
using UnityEngine.UI;

namespace Aot2Hot
{
    //这个类的方法都改为携程，等全部下载完，补充元数据后再考虑使用unitask
    public partial class Aot2HotMgr : MonoBehaviour
    {
        public TMP_Text tip;
        public Image progerssImg;
        private void Awake()
        {
            tip.text = "加载中... 请稍等";
            progerssImg.fillAmount = 0;
            StartCoroutine(nameof(IEAwake));
        }
        IEnumerator IEAwake()
        {
            yield return YooAssets.LoadAllAssetsAsync<Object>("Assets/Res/Aot2Hot/Font/JTFont.ttf");
            StartCoroutine(nameof(DownloadHotRes));
        }

        void OnDownloadProgress(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
        {
            tip.text = $"正在下载({currentDownloadCount}/{totalDownloadCount}): {(currentDownloadBytes >> 10)}KB/{(totalDownloadBytes >> 10)}KB";
            if (totalDownloadBytes > 0) {
                progerssImg.fillAmount = 1.0f * currentDownloadBytes / totalDownloadBytes;
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

            Debug.Log($"成功加载热更程序集和元数据");
            yield return YooAssets.LoadSceneAsync("Assets/Res/HotScene/HotInit.unity");
        }
    }
}
