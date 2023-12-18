using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Linq;
using YooAsset;
using HybridCLR;
using System.Reflection;
using Aot;

namespace Aot2Hot
{
    public partial class Aot2HotMgr : MonoBehaviour
    {
        public TMP_Text tip;
        void Awake()
        {
            DownloadAot2HotRes().Forget();
        }

        void OnDownloadProgress(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
        {
            tip.text = $"正在下载({currentDownloadCount}/{totalDownloadCount}): {(currentDownloadBytes >> 10)}KB/{(totalDownloadBytes >> 10)}KB";
        }
        void TryReDownload() {
            DownloadAot2HotRes().Forget();
        }
        void DownLoadEnd()
        {
            GotoHotScene().Forget();
        }
        async UniTaskVoid GotoHotScene()
        {
            //// 注意：location只需要填写资源包里的任意资源地址。
            var rawAotOp = YooAssets.LoadAllAssetsAsync<TextAsset>("Assets/Res/Raw/Aot/mscorlib.bytes");
            await rawAotOp.ToUniTask();

            // Editor下无需加载，直接查找获得HotUpdate程序集
            foreach (var asset in rawAotOp.AllAssetObjects) {
#if !UNITY_EDITOR
                var err = RuntimeApi.LoadMetadataForAOTAssembly(((TextAsset)asset).bytes, HomologousImageMode.SuperSet);
                Debug.Log($"LoadMetadataForAOTAssembly:{asset.name}. ret:{err}");
#endif
            }
            rawAotOp.Release();

            var rawHotOp = YooAssets.LoadAllAssetsAsync<TextAsset>("Assets/Res/Raw/Hot/Hot.bytes");
            await rawHotOp.ToUniTask();
            foreach (var asset in rawHotOp.AllAssetObjects) {
#if !UNITY_EDITOR
                var ass = Assembly.Load(XIHDecryptionServices.Decrypt(((TextAsset)asset).bytes));
#endif
                Debug.Log(asset.name);
            }
            rawHotOp.Release();

            Debug.Log($"成功加载热更程序集和元数据");
            await YooAssets.LoadSceneAsync("Assets/Res/HotScene/HotInit.unity").ToUniTask();
        }
    }
}
