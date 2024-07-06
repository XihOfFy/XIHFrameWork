using System;
using XiHUtil;
namespace Hot
{
    public class ChannelSDKMgr:Singleton<ChannelSDKMgr>
    {
        public readonly IChannelSDK sdkBase;
        public ChannelSDKMgr() {
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
