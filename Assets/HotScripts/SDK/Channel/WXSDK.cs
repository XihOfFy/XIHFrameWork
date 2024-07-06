#if UNITY_WX
using System;
using WeChatWASM;
namespace Hot
{
    class WXSDK : IChannelSDK
    {
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