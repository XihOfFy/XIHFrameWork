using System;
namespace Hot
{
    public static class ChannelSDKMgr
    {
        public static readonly IChannelSDK sdkBase;
        static ChannelSDKMgr() {
#if UNITY_EDITOR
            sdkBase = new InternalSDK();
#elif UNITY_WX
            sdkBase = new WXSDK();
#endif
        }
    }
    public interface IChannelSDK
    {
        public void Init(Action<bool> initCallback);
        //自行扩展其他sdk功能，例如登录，防沉迷，客服，充值等等
    }
    class InternalSDK : IChannelSDK
    {
        void IChannelSDK.Init(Action<bool> initCallback)
        {
            initCallback?.Invoke(true);
        }
    }
}
