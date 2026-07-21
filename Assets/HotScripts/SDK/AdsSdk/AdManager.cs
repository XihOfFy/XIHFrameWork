using Cysharp.Threading.Tasks;
using Hot;
using System;
using Tmpl;
using UnityEngine;
using XiHUtil;

namespace Ad
{
    public class AdManager : MonoBehaviour, IAdAdapter
    {
        private IAdAdapter _adapter;
        //public static AdManager Instance() => Singleton.Get<AdManager>(true);

        private static AdManager instance;
        public static AdManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var obj = new GameObject(nameof(AdManager));
                    instance = obj.AddComponent<AdManager>();
                    DontDestroyOnLoad(obj);
                }
                return instance;
            }
        }

        public static void Create()
        {
            var manager = Instance;
            manager.InitSDK();
            manager.InitCallBack();
            manager.showForceReward = TbApp.AppCfg.ForceReward;
            manager.needForceGap = TbApp.AppCfg.ForceGap;
            if (TbApp.AppCfg.Force60Sec)
            {
                manager.lastInsertAdTime = SyncTimeHelper.GetSystemTimeSeconds() + 60;
            }
        }
        public void InitSDK()
        {

#if UNITY_EDITOR || USE_GM
            _adapter = new LocalAdapter();
#elif UNITY_HW_QG && UNITY_WEBGL //优先华为QG ，然后使用指色上报日志
            _adapter = new HuaweiQGAdAdapter();
#elif USE_ZSSDK //指色全使用他的AD api
            _adapter = new SeegAdAdapter();
#elif TOPONSDK_AD
            _adapter = new ToponAdSdk();
#elif UNITY_WX && UNITY_WEBGL
            _adapter = new WxAdAdapter();
#elif UNITY_DY
            _adapter = new DYAdAdapter();
#elif UNITY_OPPO
            _adapter = OppoAdAdapter.Instance;
#elif UNITY_KS
            _adapter = new KsAdAdapter();
#elif UNITY_OVERSEA_NIUKOU
            _adapter = new NiuKouAdAdapter();
#elif UNITY_TT
            _adapter = new TTAdAdapter();
#else
            _adapter = new LocalAdapter();
#endif
            _adapter.InitSDK();
            adCalling = false;
        }
        public bool IsHaveReadyAd()
        {
            return _adapter.IsHaveReadyAd();
        }

        public void RemoveCallBack()
        {
            _adapter.RemoveCallBack();
        }
        public void CloseNativeAd()
        {
            _adapter.CloseNativeAd();
        }
        public void InitCallBack()
        {
            _adapter.InitCallBack();
        }

        public void ShowRewardAdv(Action<bool> onLoad, VideoSceneEnum scene, int pLevel = 0, int pProcess = 0)
        {
            if (CanCallAd())
            {
                void TmpAct(bool res)
                {
                    adCalling = false;
                    if (res)
                    {
                        if (GameBase.Instance)
                        {
                            GameBase.Instance.AdSucessEnd();
                        }
                    }
                    onLoad?.Invoke(res);
                }
                _adapter.ShowRewardAdv(TmpAct, scene);
            }
            else
            {
                UIUtil.ShowSystemTip(750001.Translate() + "...");
                onLoad?.Invoke(false);
            }
        }
        public void ShowInsertAdv(Action<bool> onLoad, VideoSceneEnum comment)
        {
            _adapter.ShowInsertAdv(onLoad, comment);
        }

        long lastInsertAdTime;
        bool showForceReward = true; // 是否为强弹激励
        bool needForceGap = true; // 是否有时间间隔
        public void ShowForceAdv(VideoSceneEnum comment)
        {
            var curTime = SyncTimeHelper.GetSystemTimeSeconds();
            if (DataSave.Instance.stageId < Tables.Instance.TbParam.ForcedAdStage) return;
            bool passGapCheck = true;
            if (lastInsertAdTime + Tables.Instance.TbParam.ForcedAdTime > curTime)
            {
                passGapCheck = false;
                if (needForceGap) return; // 播放间隔未冷却
            } // 判断是否满足间隔
            if (passGapCheck) lastInsertAdTime = curTime; // 同步记录时间
#if USE_GM
            Debug.LogWarning($"播放强制广告,广告类型:{(showForceReward ? "强弹激励" : "插屏")},是否间隔60s:{needForceGap} 渠道:{TbApp.AppCfg.ID}");
#endif
            if (showForceReward) ShowRewardAdv(null, comment);
            else ShowInsertAdv(null, comment);
        }

        //bool clickedAd;
        bool adCalling;
        bool CanCallAd()
        {
            if (adCalling) return false;
            ResetTimeout().Forget();
            return true;
        }
        async UniTaskVoid ResetTimeout()
        {
            adCalling = true;
            await UniTask.Delay(2048);
            adCalling = false;
        }
    }
}
