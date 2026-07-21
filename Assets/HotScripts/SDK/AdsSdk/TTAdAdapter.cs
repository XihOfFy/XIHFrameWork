#if UNITY_TT
using Hot;
using System;
using Tmpl;
using TTSDK;
using UnityEngine;

namespace Ad
{
    public class TTAdAdapter : IAdAdapter
    {
        void IAdAdapter.CloseNativeAd() { }
        void IAdAdapter.InitCallBack() { }
        bool IAdAdapter.IsHaveReadyAd() { return true; }
        void IAdAdapter.RemoveCallBack() { }


        TTRewardedVideoAd _rewardedAd;
        Action<bool> _rewardCallback;

        TTInterstitialAd _interstitialAd;
 

        public void InitSDK()
        {
            PreloadRewardedAd();
        }
        public void ShowInsertAdv(Action<bool> onLoad, VideoSceneEnum comment)
        {
            // 创建新的插屏广告实例
            _interstitialAd = TT.CreateInterstitialAd(new CreateInterstitialAdParam
            {
                InterstitialAdId = TbApp.AppCfg.InterstitialAd1
            });

            _interstitialAd.OnClose += () =>
            {
                Debug.Log("插屏广告已关闭");
                _interstitialAd = null;
                onLoad?.Invoke(true);
            };

            _interstitialAd.OnError += (code, msg) =>
            {
                Debug.LogError($"插屏广告错误: {code}, {msg}");
                _interstitialAd = null;
            };

            // 显示广告
            _interstitialAd.Show();
        }
        public void ShowRewardAdv(Action<bool> onLoad, VideoSceneEnum comment, int pLevel = 0, int pProcess = 0)
        {
            _rewardCallback = onLoad;
            if (_rewardedAd == null)
            {
                PreloadRewardedAd();
            }
            _rewardedAd?.Show();

        }
        void PreloadRewardedAd()
        {
            _rewardedAd = TT.CreateRewardedVideoAd(new CreateRewardedVideoAdParam { AdUnitId = TbApp.AppCfg.RewardAd1 });
            _rewardedAd.OnClose += (isEnded) =>
            {
                var act = _rewardCallback;
                _rewardCallback = null;
                _rewardedAd = null;
                act?.Invoke(isEnded);
                // 重新预加载
                PreloadRewardedAd();
            };
            _rewardedAd.OnError += (code, msg) =>
            {
                Debug.LogError($"广告错误: {code}, {msg}");
                var act = _rewardCallback;
                _rewardCallback = null;
                _rewardedAd = null;
                act?.Invoke(false);
            };
        }
    }
}
#endif