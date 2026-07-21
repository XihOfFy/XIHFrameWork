#if UNITY_KS

using System;
using Cysharp.Threading.Tasks;
using Hot;
using KSWASM;
using Tmpl;

namespace Ad
{
    public class KsAdAdapter : IAdAdapter
    {
        public void InitCallBack()
        {

        }

        public void TestAd(Action<bool> onLoad,string comment)
        {
            LoadVideo(onLoad, TbApp.AppCfg.RewardAd1);
        }

        public void InitSDK()
        {
        }

        async void OnRewardAsync(Action<bool> onLoad, bool isGet)
        {
            await UniTask.SwitchToMainThread();
            onLoad?.Invoke(isGet);
        }

        public async void LoadVideo(Action<bool> onLoad, string adUnitId)
        {
            if (string.IsNullOrEmpty(adUnitId)) {
                onLoad(true);
                return;
            }
            UnityEngine.Debug.Log($"加载广告 {adUnitId}");
            var ad = KS.CreateRewardedVideoAd(adUnitId);

            try
            {
                ad.OnClose(p => {
                    UnityEngine.Debug.Log($"关闭广告 {UnityEngine.JsonUtility.ToJson(p)} {p.isEnded}");
                    OnRewardAsync(onLoad, p.isEnded);
                });
                ad.Show(rsp => {
                    UnityEngine.Debug.Log($"展示广告...{rsp.errCode}:{rsp.errMsg}");
                });
                ad.OnError(rsp => {
                    UnityEngine.Debug.LogError($"加载广告失败 OnError:{rsp.code}:{rsp.msg}");

                });
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }

        public bool IsHaveReadyAd()
        {
            return false;
        }

        public void RemoveCallBack()
        {
        }

        public void CloseNativeAd()
        {
        }
    }
}
#endif
