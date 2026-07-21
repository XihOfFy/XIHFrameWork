#if UNITY_OPPO
using Cysharp.Threading.Tasks;
using Hot;
using System;
using Tmpl;
using UnityEngine;

namespace Ad
{
    public class OppoAdAdapter : MonoBehaviour, IAdAdapter
    {
        public void ShowRewardAdv(Action<bool> onLoad, VideoSceneEnum comment, int pLevel=0, int pProcess = 0)
        {
            LoadVideo(onLoad, TbApp.AppCfg.RewardAd1);
        }






        private static OppoAdAdapter instance;
        public static OppoAdAdapter Instance
        {
            get
            {
                if (instance == null)
                {
                    var obj = new GameObject(nameof(OppoAdAdapter));
                    instance = obj.AddComponent<OppoAdAdapter>();
                    DontDestroyOnLoad(obj);
                }
                return instance;
            }
        }
        void IAdAdapter.InitCallBack()
        {
            OppoUnitySupportAndroid.Instance().SetAdListener(this.gameObject.name);
        }

        void IAdAdapter.InitSDK()
        {

        }

        bool IAdAdapter.IsHaveReadyAd()
        {
            return true;
        }

        private Action<bool> OnReward;

        public void onLoaded(string adUnitId)
        {
            Debug.Log("Developer callback onLoaded :" + adUnitId);
        }
        //广告加载失败
        public void onFailed(string msg)
        {
            Debug.Log("Developer callback onFailed :" +  "--msg:" + msg);
            OnRewardAsync(false);
        }

        public void onReward(string msg)
        {

            Debug.Log("Developer onReward------" + "->" + msg);
            OnRewardAsync(true);
        }

        public void onShow(string adUnitId)
        {
            Debug.Log("Developer onShow------" + "->" + adUnitId);
        }


        public void onClicked(string adUnitId)
        {
            Debug.Log("Developer onClicked------" + "->" + adUnitId);
        }


        public void onClosed(string adUnitId)
        {
            Debug.Log("Developer onClosed------" + "->" + adUnitId);
            OnRewardAsync(false);
        }

        public void onSkip(string adUnitId)
        {
            Debug.Log("Developer onSkip------" + "->" + adUnitId);
            OnRewardAsync(false);
        }

        async void OnRewardAsync(bool isGet)
        {
            await UniTask.SwitchToMainThread();
            var temp = OnReward;
            OnReward = null;
            temp?.Invoke(isGet);
        }

        /// <summary>
        /// 加载广告
        /// </summary>
        /// <param name="onLoad"></param>
        /// <param name="adUnitId">oppo 此参数无用，只用日志打印区分是哪个广告触发</param>
        void LoadVideo(Action<bool> onLoad, string adUnitId)
        {
            if (string.IsNullOrEmpty(adUnitId))
            {
                onLoad(true);
                return;
            }
            UnityEngine.Debug.Log($"加载广告 {adUnitId}");
            this.OnReward = onLoad;
            OppoUnitySupportAndroid.Instance().ShowVideo(adUnitId);
        }

        void LoadBanner(string adUnitId)
        {
            UnityEngine.Debug.Log($"加载广告 {adUnitId}");
            OppoUnitySupportAndroid.Instance().showBanner(adUnitId);
        }
        void HideBanner()
        {
            OppoUnitySupportAndroid.Instance().hideBanner();
        }

        void LoadInter(string adUnitId)
        {
            UnityEngine.Debug.Log($"加载广告 {adUnitId}");
            OppoUnitySupportAndroid.Instance().showInter(adUnitId);
        }

        void LoadNativeTemp(string adUnitId)
        {
            UnityEngine.Debug.Log($"加载原生模板广告 {adUnitId}");
            OppoUnitySupportAndroid.Instance().showNativeTemp(adUnitId);
        }
        void IAdAdapter.RemoveCallBack()
        {
        }

        public void CloseNativeAd()
        {

        }

        public void ShowInsertAdv(Action<bool> onLoad, VideoSceneEnum comment)
        {

        }
    }
}
#endif
