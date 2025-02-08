#if UNITY_WX
using System;
using UnityEngine;
using WeChatWASM;

namespace Ad
{
    public class WxAdSdk : IAdSDK
    {
        const int maxTryLoadCount = 3;
        public void InitSDK()
        {
            //激励视频广告组件是自动拉取广告并进行更新的。在组件创建后会拉取一次广告，用户点击 关闭广告 后会去拉取下一条广告。
            var ad = WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam()
            {
                adUnitId = "adUnitId" //填写自己游戏的激励adID
            });
            ad.OnLoad(rsp =>
            {
                Debug.LogWarning("预加载一个广告");
                ad.OffLoad(null);
            });
            //ad.Load();//预加载一个,CreateRewardedVideoAd自动会预加载
        }

        public void ShowRewardedAd(Action<bool> onLoad, string adUnitId)
        {
            UnityEngine.Debug.Log($"加载广告 {adUnitId}");
            if (string.IsNullOrEmpty(adUnitId))
            {
                onLoad(true);
                return;
            }
            var tryLoadCount = 0;
            var ad = WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam()
            {
                adUnitId = adUnitId
            });
            try
            {
                ad.OnClose(p =>
                {
                    UnityEngine.Debug.Log($"关闭广告 {UnityEngine.JsonUtility.ToJson(p)} {p.isEnded}");
                    onLoad(p.isEnded);
                });
                ad.OnError(rsp =>
                {
                    UnityEngine.Debug.LogError($"加载广告{tryLoadCount}次失败 OnError:{rsp.callbackId}>{rsp.errCode}:{rsp.errMsg} ，将预加载下一个广告");
                    ReLoad();
                });
                ad.Show(s =>
                {
                    UnityEngine.Debug.Log("展示广告");
                }, f =>
                {
                    UnityEngine.Debug.LogError($"加载广告失败 Show:{f.callbackId}>{f.errCode}:{f.errMsg}");
                    ReLoad();
                });
                void ReLoad()
                {
                    if (tryLoadCount < maxTryLoadCount)
                    {
                        tryLoadCount += 1;
                        ad.Load(s2 =>
                        {
                            UnityEngine.Debug.Log($"加载广告 {tryLoadCount} 次失败后重新 load 广告 成功");
                            ad.Show(s3 =>
                            {
                                UnityEngine.Debug.Log($"重新尝试展示广告 {tryLoadCount} ");
                            }, f3 =>
                            {
                                UnityEngine.Debug.LogError($"重新尝试 {tryLoadCount}展示广告失败!!!");
                                ReLoad();
                            });
                        }, f2 =>
                        {
                            UnityEngine.Debug.LogError($"加载广告 {tryLoadCount}次 失败后重新 load 广告 失败");
                            ReLoad();
                        });
                    }
                    else
                    {
                        onLoad(false);
                    }
                }
            }
            catch (Exception e)
            {
                onLoad(false);
                UnityEngine.Debug.LogException(e);
            }
        }


        public void LoadInterstitialAd(string adUnitId)
        {
            UnityEngine.Debug.Log($"加载插屏广告 {adUnitId}");
            var tryLoadCount = maxTryLoadCount - 1;//只重新测试一次
            var ad = WX.CreateInterstitialAd(new WXCreateInterstitialAdParam()
            {
                adUnitId = adUnitId
            });
            try
            {
                ad.OnClose(() =>
                {
                    UnityEngine.Debug.Log($"关闭插屏广告 ");
                });

                ad.OnError(rsp =>
                {
                    UnityEngine.Debug.LogError($"加载插屏广告失败 OnError:{rsp.callbackId}>{rsp.errCode}:{rsp.errMsg} ，加载下一个广告");
                    ReLoad();
                });
                ad.Show(s =>
                {
                    UnityEngine.Debug.Log("展示插屏广告");
                }, f =>
                {
                    UnityEngine.Debug.LogError($"加载插屏广告失败 Show:{f.callbackId}>{f.errCode}:{f.errMsg}");
                    ReLoad();
                });
                void ReLoad()
                {
                    if (tryLoadCount < maxTryLoadCount)
                    {
                        tryLoadCount += 1;
                        ad.OnLoad(rsp =>
                        {
                            UnityEngine.Debug.Log("加载插屏广告...");
                            ad.Show(s =>
                            {
                                UnityEngine.Debug.Log("展示插屏广告");
                            }, f =>
                            {
                                UnityEngine.Debug.LogError($"加载插屏广告失败 Show:{f.callbackId}>{f.errCode}:{f.errMsg}");
                                ReLoad();
                            });
                        });
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }
    }
}
#endif
