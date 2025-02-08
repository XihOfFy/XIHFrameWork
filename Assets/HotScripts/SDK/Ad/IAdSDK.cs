using System;

namespace Ad
{
    public interface IAdSDK 
    {
        public void InitSDK();
        void ShowRewardedAd(Action<bool> onLoad, string comment);
    }
}
