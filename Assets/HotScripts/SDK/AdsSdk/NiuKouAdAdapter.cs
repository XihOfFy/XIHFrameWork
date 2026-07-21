#if UNITY_OVERSEA_NIUKOU
using Hot;
using QQSDK;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using XiHSound;
using XiHUtil;

namespace Ad
{
    public class NiuKouAdAdapter : IAdAdapter
    {
        Action<bool> onLoadAct;
        public void CloseNativeAd()
        {
        }

        public void InitCallBack()
        {
        }
        private void Log(object obj)
        {
            Debug.Log("=====Unity ADS:" + obj.ToString());
        }
        public void InitSDK()
        {
            AdsManager.instance.onImpressionDataCallback += (args) =>
            {
                Log("用户级别的数据展示" + args);

            };

            AdsManager.instance.onRewardSecretKeyCallback += (secretkey) =>
            {
                Log("激励密钥返回" + secretkey);
            };

            AdsManager.instance.onAttributionDataCallback += (att_data) =>
            {
                Log("归因数据回传" + JsonMapper.ToJson(att_data));
            };


            AdsManager.instance.onAbnormalUserCallback += (jsonStr) =>
            {
                Log("第三方检测用户信息返回" + jsonStr);
            };

            AdsManager.instance.onAnalyticsCallback += (network) =>
            {
                Log("IOSAdjustnetwork信息:" + network);
            };

            AdsManager.instance.onBannerImpression += () =>
            {
                Log("Banner显示成功");
            };
            AdsManager.instance.onBannerClick += () =>
            {
                Log("Banner点击");
            };
            /*if (AdsManager.instance.IsAvailableInterstitial())
            {
                interstitialBtn.interactable = true;
            }*/
            //if (AdsManager.instance.IsAvailableRewardVideo())
            //{
            //    rewardVideoBtn.interactable = true;
            //}
            AdsManager.instance.onGetAdvertismentID = (x) =>
            {
                Log("获取到的广告id" + x);
            };
            AdsManager.instance.onBannerLoadedFailed += () =>
            {
                Log("Banner加载失败");
            };
            AdsManager.instance.onBannerImpression += () =>
            {
                /*if (bannerBtn != null)
                {
                    bannerBtn.interactable = false;
                }*/
                // if (hideBannerBtn != null)
                // {
                //     hideBannerBtn.interactable = true;
                // }
            };
            AdsManager.instance.onInterstitialLoadedFailed += () =>
            {
                Log("插屏广告加载失败");
            };
            AdsManager.instance.onInterstitialOpen += () =>
            {
                Log("插屏广告打开");
                //interstitialBtn.interactable = false;
            };
            AdsManager.instance.onInterstitialClose += () =>
            {
                Log("插屏广告关闭");
                BackgroundMusic(true);
            };
            AdsManager.instance.onInterstitialClick += () =>
            {
                Log("插屏广告点击");
            };

            AdsManager.instance.onInterstitialLoaded += () =>
            {
                Log("插屏广告加载成功");
                //interstitialBtn.interactable = true;
            };
            AdsManager.instance.onIntersPageLoadedFailed += () =>
            {
                Log("插页式插屏广告加载失败");
            };
            AdsManager.instance.onIntersPageOpen += () =>
            {
                Log("插页式插屏广告打开");
                //hideBannerBtn.interactable = false;
            };
            AdsManager.instance.onIntersPageClose += () =>
            {
                Log("插页式插屏广告关闭");
            };
            AdsManager.instance.onIntersPageClick += () =>
            {
                Log("插页式插屏广告点击");
            };

            AdsManager.instance.onIntersPageLoaded += () =>
            {
                Log("插页式插屏广告加载成功");
                //hideBannerBtn.interactable = true;
            };
            AdsManager.instance.onRewardLoadFialed += () =>
            {
                Log("激励视频加载失败");
            };
            AdsManager.instance.onRewardLoaded += () =>
            {
                Log("激励加载成功");
                //rewardVideoBtn.interactable = true;
            };
            //AdsManager.instance.onRewardInterrupt += () =>
            AdsManager.instance.onRewardClose += e =>
            {
                Log("关闭广告:"+ e);
                if (!e.Equals("1"))
                {
                    //TODO:业务发奖励
                    OnReward(false);
                }
                /*else {
                    TrackingReportNiuKou.LogErrorLocation(int.Parse(e), "reward");
                }
                TrackingReportNiuKou.LogRewardLocation();*/
            };
            AdsManager.instance.onRewardOpen += () =>
            {
                //rewardVideoBtn.interactable = false;
                Log("激励打开");
            };
            AdsManager.instance.onRewardClick += () =>
            {
                Log("激励点击");
            };
            AdsManager.instance.onRewardPlayError += () =>
            {
                Log("激励视频播放失败");
            };
            AdsManager.instance.onUserRewarded += () =>
            {
                Log("激励用户");
                OnReward(true);
            };
            AdsManager.instance.onNativeLoaded += () =>
            {
                //nativeShowBtn.interactable = true;
                Log("Native加载成功");
            };
            AdsManager.instance.onNativeLoadedFailed += () =>
            {
                Log("Native加载失败");
            };
            AdsManager.instance.onNativeOpen += () =>
            {
                //nativeShowBtn.interactable = false;
                Log("Native打开");
            };
            AdsManager.instance.onNativeClick += () =>
            {
                Log("Native点击");
            };
            AdsManager.instance.onNativeClose += () =>
            {
                Log("Native关闭");
            };
        }

        public bool IsHaveReadyAd()
        {
            return false;
        }

        public void RemoveCallBack()
        {
        }

        public void TestAd(Action<bool> onLoad,string comment)
        {
            this.onLoadAct = onLoad;
            Log("准备展示激励广告");
            var res = AdsManager.instance.ShowRewardVideo(comment);
            if (1 == res)
            {
                //自定义参数，可以是场景名字，不像微信需要指定
                BackgroundMusic(false);
            }
            else {
                UIUtil.ShowSystemTip(750001.Translate());
                OnReward(false);
            }
        }


        //显示banner广告
        public void OnClickShowBanner()
        {
            Log("展示Banner广告");
            AdsManager.instance.ShowBanner(false);
        }
        public void OnClickHideBanner()
        {
            Log("隐藏Banner广告");
            //bannerBtn.interactable = true;
            //hideBannerBtn.interactable = false;
            AdsManager.instance.HideBanner();
        }
        /// <summary>
        /// 点击播放插屏广告
        /// </summary>
        public void OnClickShowInterstitialBtn()
        {
            Log("准备展示插屏广告");
            AdsManager.instance.ShowInterstitial("inter1");
            BackgroundMusic(false);
        }

        public void OnClickShowIntersPageBtn()
        {
            Log("准备展示Inters广告");

        }

        public void OnClickLoadNativeBtn()
        {
            Log("加载Native广告");
            AdsManager.instance.LoadNative();
        }

        public void OnClickShowNativeBtn()
        {
            Log("准备展示Native广告");
            AdsManager.instance.ShowNative("testNative", 0, 0.2, 20, 20);
            BackgroundMusic(false);
        }

        public void OnClickHideNativeBtn()
        {
            Log("隐藏Native广告没有准备好");
            //hideNativeBtn.interactable = false;
            AdsManager.instance.HideNative();
            BackgroundMusic(true);
            //建议隐藏之后，加载下一个
            AdsManager.instance.LoadNative();
        }

        /// <summary>
        /// 广告开关的时候，要处理背景音乐的停止和播放逻辑
        /// onInterstitialClose
        /// onRewardComplete
        /// onRewardInterrupt  这三个开启
        ///      ShowRewardAd
        ///      ShowInterisitalAd 这两个关闭
        /// </summary>
        /// <param name="isMu"></param>
        private void BackgroundMusic(bool isMu)
        {
            Log("游戏背景音乐状态：" + isMu);
            //TODO:处理背景音乐逻辑
            if (isMu)
            {
                SoundMgr.Instance.UnPause();
            }
            else {
                SoundMgr.Instance.PauseBGM();
            }
        }


        public void OnClickRequestAdPermissionBtn()
        {
            Log("点击获取权限");
            AdsManager.instance.RequestAdPermission();
        }


        void OnReward(bool suc)
        {
            onLoadAct?.Invoke(suc);
            BackgroundMusic(true);
        }
    }
}

#endif
