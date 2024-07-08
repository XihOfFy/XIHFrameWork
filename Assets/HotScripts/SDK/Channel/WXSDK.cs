#if UNITY_WX
using System;
using UnityEngine;
using WeChatWASM;
namespace Hot
{
    class WXSDK : IChannelSDK
    {
        public void TouchOverride(GameObject dotDestoryObj)
        {
            dotDestoryObj.AddComponent<WXTouchInputOverride>();//这里会涉及隐私WX.GetSystemInfoSync().platform，所以建议放在CheckPrivacy之后执行，且这里还会执行一次WX.InitSDK
        }

        void IChannelSDK.Init(Action<bool> initCallback)
        {
            WX.InitSDK(code => {
                UnityEngine.Debug.Log($"InitSDK... code: {code}");
                initCallback?.Invoke(true);//自己写结果判断
            });
        }
    }
}
#endif