using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using WeChatWASM;
using YooAsset;

namespace Hot
{
    public class PlatformUtil
    {
        static bool gcing = false;
        public static async UniTaskVoid TriggerGC() {
            if (gcing) return;
            gcing = true;
            await UniTask.Yield();
            gcing = false;
            YooAssets.GetPackage(Aot.AotConfig.PACKAGE_NAME).UnloadUnusedAssets();
            GC.Collect();
#if UNITY_WX
            WeChatWASM.WX.TriggerGC();
#endif
        }
        public static void SetFramePerSecond(int frame) {
#if UNITY_WX
            WX.SetPreferredFramesPerSecond(frame);//��Щios�ֻ�30֡���ܻ���ֻ����������
#else
            Application.targetFrameRate = frame;
#endif
        }

        internal static void Vibrate()
        {
#if UNITY_WX
            WX.VibrateShort(new VibrateShortOption() { type = "medium" });
#elif UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#else
            Debug.LogWarning("��ǰƽ̨��֧����");
#endif
        }
    }
}
