#if UNITY_DY && !USE_ZSSDK
using Cysharp.Threading.Tasks;
using Hot;
using System;
using Tmpl;
using TTSDK;
using XiHSound;

namespace Ad
{
    public class DYAdAdapter : IAdAdapter
    {
        TTRewardedVideoAd rewardedVideoAd;
        TTInterstitialAd interstitialAd;
        Action<bool> rewardedVideoAdAct;
        public void ShowRewardAdv(Action<bool> onLoad, VideoSceneEnum comment, int pLevel = 0, int pProcess = 0)
        {
            rewardedVideoAdAct = onLoad;
#if UNITY_ANDROID
            SoundMgr.Instance.PauseBGM();
#endif
            //抖音广告多次调用，只会回调最初的onLoad，后面的都会舍弃掉
            //抖音不能设置超时，因为 TTSDK.TT.GetStarkAdManager()没有广告开始播放的事件，不然会先回调timeout，然后回调其他导致出现可能丢失奖励问题
            //var loadDialog = await XiHUtil.UIUtil.OpenDialogAsync<LoadTipDialog>();
            //loadDialog.Show("广告正在加载中...", 8);
            try
            {
                rewardedVideoAd.Show();
            }
            catch (Exception e)
            {
                OnRewardAsync(false);
                //TrackingReport.SpecEventLogReq("adv_fail");
                UnityEngine.Debug.LogException(e);

                PrepareRewardAd();
            }
            //loadDialog.Close();
        }

        public void ShowInsertAdv(Action<bool> onLoad, VideoSceneEnum comment)
        {
            if (interstitialAd != null) interstitialAd.Show();
            onLoad?.Invoke(true);
        }
        void IAdAdapter.InitCallBack()
        {
        }
        void IAdAdapter.InitSDK()
        {
            PrepareRewardAd();
            PrepareInterstitialAd();
            StarkSDKSpace.StarkAdManager.IsShowLoadAdToast = false;
        }
        void PrepareRewardAd()
        {
            rewardedVideoAd = TTSDK.TT.CreateRewardedVideoAd(TbApp.AppCfg.RewardAd1, (isComplete, code) => {
                UnityEngine.Debug.Log($"关闭广告: {isComplete}");
                OnRewardAsync(isComplete);
                Aot.AOTToufangSDK.ReportAd(isComplete);
                //TrackingReport.SpecEventLogReq(isComplete?"adv_success":"adv_exit");
            }, (code, err) => {
                UnityEngine.Debug.Log($"广告出错: {code} {err}");
                OnRewardAsync(false);
                //TrackingReport.SpecEventLogReq("adv_fail");

                PrepareRewardAd();
            });
            rewardedVideoAd.Load();//说明：开发者可以调用 videoAd.load 用于广告加载，但建议直接使用 show 进行展示，而不用 load->触发 onLoad->show 的链路
        }
        void PrepareInterstitialAd()
        {
            if (!string.IsNullOrEmpty(TbApp.AppCfg.InterstitialAd1))
            {
                interstitialAd = TTSDK.TT.CreateInterstitialAd(new CreateInterstitialAdParam()
                {
                    InterstitialAdId = TbApp.AppCfg.InterstitialAd1
                });
                interstitialAd.Load();//说明：开发者可以调用 videoAd.load 用于广告加载，但建议直接使用 show 进行展示，而不用 load->触发 onLoad->show 的链路
            }
        }
        bool IAdAdapter.IsHaveReadyAd()
        {
            return true;
        }

        async void OnRewardAsync(bool isGet)
        {
            await UniTask.SwitchToMainThread();
            rewardedVideoAdAct?.Invoke(isGet);
#if UNITY_ANDROID
            SoundMgr.Instance.UnPause();
#endif
        }
        void IAdAdapter.RemoveCallBack()
        {
        }
        void IAdAdapter.CloseNativeAd()
        {
        }
    }
}
#endif
