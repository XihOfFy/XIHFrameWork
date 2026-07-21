#if USE_ZSSDK
using System;
using ZhiSe;
using Tmpl;
using XiHUtil;
using Cysharp.Threading.Tasks;
using XiHSound;
using Hot;
using UnityEngine;

namespace Ad
{
    public class SeegAdAdapter : IAdAdapter
    {
        void IAdAdapter.CloseNativeAd() { }
        void IAdAdapter.InitCallBack() { }
        void IAdAdapter.RemoveCallBack() { }
        bool IAdAdapter.IsHaveReadyAd() { return true; }

        public void InitSDK()
        {
            //Seeg.InitSdk();和投放Init重复
            //Seeg.InitBannerAd();
            //Seeg.InitInterstitialAd();
#if UNITY_WEBGL //小游戏平台需要自己设置adunitid
            var param_reward = new RewardedAdInitParam()
            {
                adUnitId = TbApp.AppCfg.RewardAd1
            };
            var param_insert = new InterstitialAdInitParam()
            {
                adUnitId = TbApp.AppCfg.InterstitialAd1
            };
#else
            RewardedAdInitParam param_reward = null;
            InterstitialAdInitParam param_insert = null;
#endif
            Seeg.InitRewardedVideoAd(param_reward);
            Seeg.InitInterstitialAd(param_insert);
        }

        public void ShowRewardAdv(Action<bool> onLoad, VideoSceneEnum comment, int pLevel=0, int pProcess = 0)
        {
            //AdManager.Instance.StopBGM(4000).Forget();
            RewardedAdShowParam param = new()
            {
                positionTag = comment.ToString().ToLower(),
                timeout = 3000,
                success = (res) =>
                {
                    SoundMgr.Instance.UnPause();
                    onLoad(true);
                },
                fail = (res) =>
                {
                    SoundMgr.Instance.UnPause();
                    onLoad(false);
                },
                start = () => { 
                    SoundMgr.Instance.PauseBGM();
                }
            };
            Seeg.ShowRewardedVideoAd(param);
        }

        public void ShowInsertAdv(Action<bool> onLoad, VideoSceneEnum comment)
        {
            if(string.IsNullOrEmpty(TbApp.AppCfg.InterstitialAd1))
            {
                Debug.LogError("插屏广告adunitid为空,跳过展示");
                onLoad?.Invoke(true);
                return;
            }
            InterstitialAdShowParam param = new()
            {
                positionTag = comment.ToString().ToLower(),
                timeout = 2000,
                start = () => { SoundMgr.Instance.PauseBGM(); },
                fail = (e) => 
                {
                    SoundMgr.Instance.UnPause();
                    onLoad?.Invoke(false); 
                },
                success= () =>
                {
                    SoundMgr.Instance.UnPause();
                    onLoad?.Invoke(true);
                }
            };
            Seeg.ShowInterstitialAd(param);
        }
    }
}
#endif