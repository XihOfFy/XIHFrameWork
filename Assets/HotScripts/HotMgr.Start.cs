using XiHUtil;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Aot.XiHUtil;

namespace Hot
{
    public partial class HotMgr
    {
        void CheckGuide()
        {
            var stageId = DataSave.Instance.stageId;
            TrackingReport.TrackLoadComplete();
            PlatformUtil.RegisterLowMemoryEvent(LowMemoryCallback);
            UIUtil.LoadHomeScene().Forget();
            //PreLoadShader().Forget();
        }
        void LowMemoryCallback()
        {
            PlatformUtil.TriggerGC();
        }
        async UniTaskVoid InitShaderVariants()
        {
            var handle = AssetLoadUtil.LoadAssetAsync<ShaderVariantCollection>("Assets/Res/ShaderVariants/ShaderVariants.shadervariants");
            await handle.ToUniTask();
            var asset = handle.GetAsset<ShaderVariantCollection>();
            asset.WarmUp();
            handle.Release();
        }
    }
}
