#if UNITY_DY
using System;
using TTSDK;
using XiHSound;

namespace Ad
{
    public class DYAdSdk : IAdSDK
    {
        TTRewardedVideoAd rewardedVideoAd;
        Action<bool> rewardedVideoAdAct;
        public void InitSDK()
        {
            //填写自己游戏的激励adID
            StarkSDKSpace.StarkAdManager.IsShowLoadAdToast = false;
            rewardedVideoAd = TTSDK.TT.CreateRewardedVideoAd("adUnitId", (isComplete, code) =>
            {
                UnityEngine.Debug.Log($"关闭广告: {isComplete}");
                OnRewardAsync(isComplete);
            }, (code, err) =>
            {
                UnityEngine.Debug.Log($"广告出错: {code} {err}");
                OnRewardAsync(false);
            });
            rewardedVideoAd.Load();//说明：开发者可以调用 videoAd.load 用于广告加载，但建议直接使用 show 进行展示，而不用 load->触发 onLoad->show 的链路
        }
        void OnRewardAsync(bool isGet)
        {
            rewardedVideoAdAct?.Invoke(isGet);
#if UNITY_ANDROID
            SoundMgr.Instance.UnPause();
#endif
        }
        public void ShowRewardedAd(Action<bool> onLoad, string adUnitId)
        {
            if (string.IsNullOrEmpty(adUnitId))
            {
                onLoad(true);
                return;
            }
            rewardedVideoAdAct = onLoad;
#if UNITY_ANDROID
            SoundMgr.Instance.PauseBGM();
#endif
            //抖音广告多次调用，只会回调最初的onLoad，后面的都会舍弃掉
            //抖音不能设置超时，因为 TTSDK.TT.GetStarkAdManager()没有广告开始播放的事件，不然会先回调timeout，然后回调其他导致出现可能丢失奖励问题
            try
            {
                rewardedVideoAd.Show();
            }
            catch (Exception e)
            {
                OnRewardAsync(false);
                UnityEngine.Debug.LogException(e);
            }
        }
    }
}
#endif