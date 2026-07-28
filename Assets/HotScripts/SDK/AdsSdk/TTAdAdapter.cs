#if UNITY_TT
using Hot;
using System;
using Tmpl;
using TTSDK;
using UnityEngine;

namespace Ad
{
    /// <summary>
    /// TikTok/抖音 IAA 广告适配：预加载只提前请求素材，展示仍由用户主动 Show 触发。
    /// 客户端版本需 >= 46.1.0 才支持 Load；调用前用 CanIUse 判断，不支持则跳过预加载，Show 仍可走实时拉取兜底。
    /// </summary>
    public class TTAdAdapter : IAdAdapter
    {
        void IAdAdapter.CloseNativeAd() { }
        void IAdAdapter.InitCallBack() { }
        void IAdAdapter.RemoveCallBack() { }

        bool IAdAdapter.IsHaveReadyAd()
        {
            // IsLoaded 仅作业务参考，不是 Show 的前置条件；有实例即可尝试展示
            return _rewardedAd != null;
        }

        TTRewardedVideoAd _rewardedAd;
        Action<bool> _rewardCallback;

        TTInterstitialAd _interstitialAd;
        Action<bool> _interstitialCallback;

        public void InitSDK()
        {
            PrepareRewardedAd();
            PrepareInterstitialAd();
        }

        public void ShowInsertAdv(Action<bool> onLoad, VideoSceneEnum comment)
        {
            _interstitialCallback = onLoad;
            if (_interstitialAd == null)
            {
                PrepareInterstitialAd();
            }

            if (_interstitialAd == null)
            {
                Debug.LogError("插屏广告未准备好（广告位可能未配置）");
                _interstitialCallback = null;
                onLoad?.Invoke(false);
                return;
            }

            // 不等待 OnLoad，也不用 IsLoaded 阻止展示
            _interstitialAd.Show();
        }

        public void ShowRewardAdv(Action<bool> onLoad, VideoSceneEnum comment, int pLevel = 0, int pProcess = 0)
        {
            _rewardCallback = onLoad;
            if (_rewardedAd == null)
            {
                PrepareRewardedAd();
            }

            if (_rewardedAd == null)
            {
                Debug.LogError("激励广告未准备好");
                _rewardCallback = null;
                onLoad?.Invoke(false);
                return;
            }

            // 不等待 OnLoad，也不用 IsLoaded 阻止展示；预加载失败时客户端会走实时请求兜底
            _rewardedAd.Show();
        }

        #region 激励视频

        /// <summary>
        /// 进入可预判广告场景 / 上一实例关闭后调用：销毁旧实例，创建新实例并按能力预加载。
        /// </summary>
        void PrepareRewardedAd()
        {
            DisposeRewardedAd();

            _rewardedAd = TT.CreateRewardedVideoAd(new CreateRewardedVideoAdParam
            {
                AdUnitId = TbApp.AppCfg.RewardAd1
            });

            _rewardedAd.OnLoad += OnRewardedLoaded;
            _rewardedAd.OnError += OnRewardedError;
            _rewardedAd.OnClose += OnRewardedClosed;

            if (CanIUse.TTRewardedVideoAd.Load)
            {
                _rewardedAd.Load();
            }
        }

        void OnRewardedLoaded()
        {
            Debug.Log("激励广告预加载成功");
        }

        void OnRewardedError(int code, string msg)
        {
            Debug.LogError($"激励广告错误: {code}, {msg}");

            // 预加载失败不发奖、不销毁：保留实例供 Show 走兜底链路
            var act = _rewardCallback;
            if (act == null) return;

            // 用户已触发展示后的失败：回调并重建下一实例
            _rewardCallback = null;
            act.Invoke(false);
            DisposeRewardedAd();
            PrepareRewardedAd();
        }

        void OnRewardedClosed(bool isEnded)
        {
            // 奖励只依据关闭回调的完整观看结果，不能以预加载成功作为发奖依据
            var act = _rewardCallback;
            _rewardCallback = null;
            act?.Invoke(isEnded);

            DisposeRewardedAd();
            PrepareRewardedAd();
        }

        void DisposeRewardedAd()
        {
            if (_rewardedAd == null) return;

            _rewardedAd.OnLoad -= OnRewardedLoaded;
            _rewardedAd.OnError -= OnRewardedError;
            _rewardedAd.OnClose -= OnRewardedClosed;
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        #endregion

        #region 插屏

        void PrepareInterstitialAd()
        {
            if (string.IsNullOrEmpty(TbApp.AppCfg.InterstitialAd1)) return;

            DisposeInterstitialAd();

            _interstitialAd = TT.CreateInterstitialAd(new CreateInterstitialAdParam
            {
                InterstitialAdId = TbApp.AppCfg.InterstitialAd1
            });

            _interstitialAd.OnLoad += OnInterstitialLoaded;
            _interstitialAd.OnError += OnInterstitialError;
            _interstitialAd.OnClose += OnInterstitialClosed;

            if (CanIUse.TTInterstitialAd.Load)
            {
                _interstitialAd.Load();
            }
        }

        void OnInterstitialLoaded()
        {
            Debug.Log("插屏广告预加载成功");
        }

        void OnInterstitialError(int code, string msg)
        {
            Debug.LogError($"插屏广告错误: {code}, {msg}");

            var act = _interstitialCallback;
            if (act == null) return;

            _interstitialCallback = null;
            act.Invoke(false);
            DisposeInterstitialAd();
            PrepareInterstitialAd();
        }

        void OnInterstitialClosed()
        {
            Debug.Log("插屏广告已关闭");
            var act = _interstitialCallback;
            _interstitialCallback = null;
            act?.Invoke(true);

            DisposeInterstitialAd();
            PrepareInterstitialAd();
        }

        void DisposeInterstitialAd()
        {
            if (_interstitialAd == null) return;

            _interstitialAd.OnLoad -= OnInterstitialLoaded;
            _interstitialAd.OnError -= OnInterstitialError;
            _interstitialAd.OnClose -= OnInterstitialClosed;
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        #endregion
    }
}
#endif
