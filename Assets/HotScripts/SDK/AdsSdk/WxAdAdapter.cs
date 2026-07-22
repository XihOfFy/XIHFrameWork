#if UNITY_WX
using Hot;
using System;
using Tmpl;
using UnityEngine;
using WeChatWASM;

namespace Ad
{
    /// <summary>
    /// 微信激励/插屏广告适配，结构对齐 TTAdAdapter：预加载激励、关闭后重建、插屏即用即建。
    /// </summary>
    public class WxAdAdapter : IAdAdapter
    {
        void IAdAdapter.CloseNativeAd() { }
        void IAdAdapter.InitCallBack() { }
        bool IAdAdapter.IsHaveReadyAd() { return true; }
        void IAdAdapter.RemoveCallBack() { }

        WXRewardedVideoAd _rewardedAd;
        Action<bool> _rewardCallback;

        WXInterstitialAd _interstitialAd;

        public void InitSDK()
        {
            PreloadRewardedAd();
        }

        public void ShowInsertAdv(Action<bool> onLoad, VideoSceneEnum comment)
        {
            if (string.IsNullOrEmpty(TbApp.AppCfg.InterstitialAd1))
            {
                onLoad?.Invoke(true);
                return;
            }

            // 创建新的插屏广告实例（微信建议用完即弃）
            _interstitialAd = WX.CreateInterstitialAd(new WXCreateInterstitialAdParam
            {
                adUnitId = TbApp.AppCfg.InterstitialAd1
            });

            _interstitialAd.OnClose(() =>
            {
                Debug.Log("插屏广告已关闭");
                _interstitialAd = null;
                onLoad?.Invoke(true);
            });

            _interstitialAd.OnError(rsp =>
            {
                Debug.LogError($"插屏广告错误: {rsp.errCode}, {rsp.errMsg}");
                _interstitialAd = null;
                onLoad?.Invoke(false);
            });

            _interstitialAd.Show(
                _ => Debug.Log("展示插屏广告"),
                f =>
                {
                    Debug.LogError($"插屏广告展示失败: {f.errCode}, {f.errMsg}");
                    _interstitialAd = null;
                    onLoad?.Invoke(false);
                });
        }

        public void ShowRewardAdv(Action<bool> onLoad, VideoSceneEnum comment, int pLevel = 0, int pProcess = 0)
        {
            if (string.IsNullOrEmpty(TbApp.AppCfg.RewardAd1))
            {
                onLoad?.Invoke(true);
                return;
            }

            _rewardCallback = onLoad;
            if (_rewardedAd == null)
            {
                PreloadRewardedAd();
            }

            // 微信 Show 失败时常见原因是尚未加载完成，补一次 Load 后再 Show
            _rewardedAd?.Show(
                _ => Debug.Log("展示激励广告"),
                f =>
                {
                    Debug.LogError($"激励广告展示失败，尝试重新加载: {f.errCode}, {f.errMsg}");
                    _rewardedAd?.Load(
                        _ => _rewardedAd?.Show(
                            null,
                            f2 =>
                            {
                                Debug.LogError($"激励广告重试展示失败: {f2.errCode}, {f2.errMsg}");
                                InvokeRewardCallback(false);
                            }),
                        f2 =>
                        {
                            Debug.LogError($"激励广告重新加载失败: {f2.errCode}, {f2.errMsg}");
                            InvokeRewardCallback(false);
                        });
                });
        }

        void PreloadRewardedAd()
        {
            if (string.IsNullOrEmpty(TbApp.AppCfg.RewardAd1))
                return;

            // Create 后会自动拉取一条广告
            _rewardedAd = WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam
            {
                adUnitId = TbApp.AppCfg.RewardAd1
            });

            _rewardedAd.OnClose(p =>
            {
                var isEnded = p != null && p.isEnded;
                Debug.Log($"关闭激励广告 isEnded={isEnded}");
                _rewardedAd = null;
                InvokeRewardCallback(isEnded);
                // 关闭后预加载下一条
                PreloadRewardedAd();
            });

            _rewardedAd.OnError(rsp =>
            {
                Debug.LogError($"激励广告错误: {rsp.errCode}, {rsp.errMsg}");
                _rewardedAd = null;
                InvokeRewardCallback(false);
            });
        }

        void InvokeRewardCallback(bool isEnded)
        {
            var act = _rewardCallback;
            _rewardCallback = null;
            act?.Invoke(isEnded);
        }
    }
}
#endif
