#if UNITY_HW_QG
using Cysharp.Threading.Tasks;
using Hot;
using HWWASM;
using System;
using Tmpl;
using UnityEngine;

namespace Ad
{
    public class HuaweiQGAdAdapter : IAdAdapter
    {
        //预加载操作激励视频，创建视频对象，加载视频load，监听调用onload，监听关闭onclose
        private RewardedVideoAd rewardedVideoAd;

        public void ShowRewardAdv(Action<bool> onLoad, VideoSceneEnum comment, int pLevel=0, int pProcess = 0) {
            LoadVideo(onLoad, TbApp.AppCfg.RewardAd1);
        }

        public void ShowInsertAdv(Action<bool> onLoad, VideoSceneEnum comment) {
            Debug.LogError("暂时无此类功能");
        }

        void IAdAdapter.InitCallBack()
        {
        }

        void IAdAdapter.InitSDK()
        {
            rewardedVideoAd = QG.CreateRewardedVideoAd(new CreateRewardedVideoAdOption()
            {
                adUnitId = TbApp.AppCfg.RewardAd1
            });
            rewardedVideoAd.Load();//预加载一次
        }

        bool IAdAdapter.IsHaveReadyAd()
        {
            return true;
        }

        async void OnRewardAsync(Action<bool> onLoad, bool isGet)
        {
            await UniTask.SwitchToMainThread();
            onLoad?.Invoke(isGet);
        }
        int loadCount = 0;
        public void LoadVideo(Action<bool> onLoad, string adUnitId)
        {
            if (string.IsNullOrEmpty(adUnitId)) {
                onLoad(true);
                return;
            }
            UnityEngine.Debug.Log($"加载广告 {adUnitId}");
            loadCount = 0;

            var ad = rewardedVideoAd;
            try
            {
                ad.OnClose(p => {
                    UnityEngine.Debug.Log($"关闭广告 {UnityEngine.JsonUtility.ToJson(p)} {p.isEnded}");
                    OnRewardAsync(onLoad, p.isEnded);
                    ad.OffLoad();
                    ad.OffClose();
                    ad.OffError();
                    ad.Load();//预加载下一次
                });

                ad.OnError(rsp => {
                    UnityEngine.Debug.LogError($"加载广告失败{loadCount}次 OnError:{rsp._callbackId}>{rsp.errCode}:{rsp.errMsg}");
                    if (loadCount < 3)
                    {
                        loadCount += 1;
                        ad.Load();
                    }
                    else {
                        OnRewardAsync(onLoad, false);
                    }
                });

                ad.OnLoad(() => {
                    UnityEngine.Debug.Log($"加载广告成功...");
                    ad.Show();
                });
                ad.Load();
            }
            catch (Exception e)
            {
                OnRewardAsync(onLoad, false);
                UnityEngine.Debug.LogException(e);
            }
        }
        void IAdAdapter.RemoveCallBack()
        {
        }

        public void CloseNativeAd()
        {
        }

        public void ShowGardenBattleEndInterAd()
        {

        }
    }
}

#endif
