#if UNITY_WX
using WeChatWASM;
#endif

using Aot.XiHUtil;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XiHAsset;

namespace Hot
{
    public static partial class WxTool
    {
        public static async UniTask<float> ShareAppMessage(string spritePath, string saveRelativePath, string title)
        {
#if UNITY_WX
            var end = false;
            float stayTime=0;
            float durTime = 0;
            void ShareHideEvent(GeneralCallbackResult result)
            {
                stayTime = Time.realtimeSinceStartup;
                Debug.Log("ShareHideEvent");
            }
            void ShareShowEvent(OnShowListenerResult result)
            {
                var time = Time.realtimeSinceStartup;
                durTime = time - stayTime;
                end = true;
                WeChatWASM.WX.OffHide(ShareHideEvent);
                WeChatWASM.WX.OffShow(ShareShowEvent);
                Debug.Log("ShareShowEvent");
            }
            WeChatWASM.WX.OnHide(ShareHideEvent);
            WeChatWASM.WX.OnShow(ShareShowEvent);

           var fullPath = await SaveSprite2TmpPath(spritePath,saveRelativePath);
           WeChatWASM.WX.ShareAppMessage(new ShareAppMessageOption()
            {
                imageUrl = fullPath,
                title = title
            });
            await UniTask.WaitUntil(()=>end);
            return durTime;
#else 
            return 0;
#endif
        }
    }
}
