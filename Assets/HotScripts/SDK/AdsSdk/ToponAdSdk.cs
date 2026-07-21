#if TOPONSDK_AD
using System;
using System.Collections;
using System.Collections.Generic;
using AOT.Base;
using Hot;
using Tmpl;

namespace Ad
{
    public class ToponAdSdk : IAdAdapter
    {
        public void ShowRewardAdv(Action<bool> onLoad, VideoSceneEnum comment, int pLevel=0, int pProcess = 0)
        {
            LoadVideo(onLoad);
        }










        const string mPlacementId_rewardvideo_all = "b6699d0120b8a1";

        void IAdAdapter.InitCallBack()
        {

            ToponSDKImp.getInstance().InitCallBack();
        }

        void IAdAdapter.InitSDK()
        {
            ToponSDKImp.getInstance().InitSDK();
        }
        bool IAdAdapter.IsHaveReadyAd()
        {
            return ToponSDKImp.getInstance().IsHaveReadyAd(mPlacementId_rewardvideo_all);
        }

        //public void LoadVideo(Action<bool> onLoad)
        //{
        //    ToponSDKImp.getInstance().LoadVideo(onLoad, RoleModel.Instance.roleUid, RoleModel.Instance.roleName, mPlacementId_rewardvideo_all);
        //}

        void IAdAdapter.RemoveCallBack()
        {
            ToponSDKImp.getInstance().RemoveCallBack();
        }

        string curPlacementId_native="";
        public void CloseNativeAd()
        {
            ToponSDKImp.getInstance().CloseNativeAd(curPlacementId_native);
        }
        //public void NativeFriendBattleAd(int x, int y, int width, int height)
        //{
        //    ShowTmplAd(x, y, width, height,NativeFriendBattle, mPlacementId_native_big_all);
        //}
        //public void NativeYogaAd(int x, int y, int width, int height)
        //{
        //    ShowTmplAd(x, y, width, height, NativeYoga, mPlacementId_native_small_all);
        //}
        //public void NativeWeaponTrainAd(int x, int y, int width, int height)
        //{
        //    ShowTmplAd(x, y, width, height, NativeWeaponTrain, mPlacementId_native_small_all);
        //}
        //public void NativeCommonTrainAd(int x, int y, int width, int height)
        //{
        //    ShowTmplAd(x, y, width, height, NativeCommonTrain, mPlacementId_native_small_all);
        //}
        void ShowTmplAd(int x, int y, int width, int height, string scenario,string mPlacementId_native_all) {
           /* var cfg = Tmpl.AdViewTable.Instance().Get(scenario);
            if (cfg == null) return;
            curPlacementId_native = mPlacementId_native_all;
            ToponSDKImp.getInstance().ShowNativeTmplAd(mPlacementId_native_all, x, y, width, height, cfg.bgcolor, cfg.textcolor, (height >> 4) + 1 , cfg.usesPixel, cfg.isCustomClick,  scenario, cfg.ATSizeUsesPixel);*/
        }

        public void LoadVideo(Action<bool> onLoad)
        {
            onLoad += res =>
            {
            };
            ToponSDKImp.getInstance().LoadVideo(onLoad,0,DataSave.Instance.userId, TbApp.AppCfg.RewardAd1);
        }

        public void ShowInsertAdv(Action<bool> onLoad, VideoSceneEnum comment)
        {

        }
    }
}
#endif
