using Aot;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace XiHUtil
{
    public class PlatformUtil
    {
        public static void TriggerGC()
        {
            InnerTriggerGC().Forget();
        }
        static bool gcing = false;
        static async UniTaskVoid InnerTriggerGC()
        {
            //因为可以做过度界面，所以可以即时GC
            if (gcing) return;
            gcing = true;
            await AssetLoadUtil.UnloadUnusedAssetInner();

            await UniTask.Yield();
#if UNITY_WX && !UNITY_EDITOR
            WeChatWASM.WX.TriggerGC();
#else
            GC.Collect();
#endif
            gcing = false;
        }
        public static void SetFramePerSecond(int frame)
        {
            //#if UNITY_WX && !UNITY_EDITOR
            //            WX.SetPreferredFramesPerSecond(frame);//有些ios手机30帧可能会出现画面闪屏情况
            //微信也建议直接使用Application.targetFrameRate 
#if UNITY_HW_QG
            HWWASM.QG.SetPreferredFramesPerSecond(frame);
#else
            Application.targetFrameRate = frame;
#endif
        }

        internal static void Vibrate()
        {
#if UNITY_WX && !UNITY_EDITOR
            WeChatWASM.WX.VibrateShort(new WeChatWASM.VibrateShortOption() { type = "medium" });
#elif UNITY_DY
            if (!DYStarkSDK.Instance.DY_PC) TTSDK.TT.Vibrate(new long[] {0 , 30 }, -1); //等待0s，开始震动30ms 使手机发生较短时间的振动。Android 震动时间为 30ms，iOS 震动时间为 15ms。某些机型在不支持短振动时会 fallback 到 vibrateLong，某些机型不支持时会进入 fail 回调。
#elif UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#else
            //Debug.LogWarning("当前平台不支持震动");
#endif
        }

        public static void LogError(string v)
        {
#if UNITY_EDITOR
            Debug.LogError(v);
#endif
        }
        public static void LogWarning(string v)
        {
#if UNITY_EDITOR
            Debug.LogWarning(v);
#endif
        }

        public static void Log(string v)
        {
#if UNITY_EDITOR
            Debug.Log(v);
#endif
        }

        public static void RegisterLowMemoryEvent(Application.LowMemoryCallback callBack) {
#if UNITY_WX && !UNITY_EDITOR
            WeChatWASM.WX.OnMemoryWarning(res => {
                callBack();
            });
#else
            Application.lowMemory += callBack;
#endif
        }
    }
}
