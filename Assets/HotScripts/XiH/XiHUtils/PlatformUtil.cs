#if UNITY_WX && !UNITY_EDITOR
#define UNNITY_WX_WITHOUT_EDITOR
#endif
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using YooAsset;
#if UNNITY_WX_WITHOUT_EDITOR
using WeChatWASM;
#endif

namespace XiHUtil
{
    public class PlatformUtil
    {
        public static void TriggerGC() {
            //暂时不主动触发GC，让系统自己回收
            //InnerTriggerGC().Forget();
        }
        static bool gcing = false;
        static async UniTaskVoid InnerTriggerGC() {
            if (gcing) return;
            gcing = true;
            await UniTask.Yield();
            gcing = false;
            YooAssets.GetPackage(Aot.AotConfig.PACKAGE_NAME).UnloadUnusedAssetsAsync().ToUniTask().Forget();
            GC.Collect();
#if UNNITY_WX_WITHOUT_EDITOR
            WeChatWASM.WX.TriggerGC();
#endif
        }
        public static void SetFramePerSecond(int frame) {
#if UNNITY_WX_WITHOUT_EDITOR
            WX.SetPreferredFramesPerSecond(frame);//有些ios手机30帧可能会出现画面闪屏情况
#else
            Application.targetFrameRate = frame;
#endif
        }

        internal static void Vibrate()
        {
#if UNNITY_WX_WITHOUT_EDITOR
            WX.VibrateShort(new VibrateShortOption() { type = "medium" });
#elif UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#else
            Debug.LogWarning("当前平台不支持震动");
#endif
        }
    }
}
