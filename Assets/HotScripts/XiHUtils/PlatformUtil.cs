using System;
using YooAsset;

namespace Hot
{
    public class PlatformUtil
    {
        public static void TriggerGC() {
            GC.Collect();
#if UNITY_WX
            WeChatWASM.WX.TriggerGC();
#endif
            YooAssets.GetPackage(Aot.AotConfig.PACKAGE_NAME).UnloadUnusedAssets();
        }
    }
}
