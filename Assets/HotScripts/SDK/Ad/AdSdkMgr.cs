namespace Ad
{
    public static class AdSdkMgr 
    {
        public static readonly IAdSDK sdkBase;
        static AdSdkMgr()
        {
#if UNITY_WX
            sdkBase = new WxAdSdk();
#elif UNITY_DY
            sdkBase = new DYAdSdk();
#else
            sdkBase = new LocalAdSdk();
#endif
        }
    }
}
