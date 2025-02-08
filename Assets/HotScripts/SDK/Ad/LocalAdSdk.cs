
using System;

namespace Ad
{
    public class LocalAdSdk : IAdSDK
    {
        public void InitSDK()
        {
        }
        public void ShowRewardedAd(Action<bool> onLoad, string comment)
        {
            onLoad?.Invoke(true);
        }
    }
}
