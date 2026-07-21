
using Hot;
using System;

namespace Ad
{
    public interface IAdAdapter 
    {

        void ShowRewardAdv(Action<bool> onLoad, VideoSceneEnum comment, int pLevel=0, int pProcess = 0);

        void ShowInsertAdv(Action<bool> onLoad, VideoSceneEnum comment);

        public void InitSDK();

        public void InitCallBack();

        public bool IsHaveReadyAd();

        public void RemoveCallBack();

        //public void NativeFriendBattleAd(int x,int y,int width,int height);
        //public void NativeYogaAd(int x, int y, int width, int height);
        //public void NativeWeaponTrainAd(int x, int y, int width, int height);
        //public void NativeCommonTrainAd(int x, int y, int width, int height);
        public void CloseNativeAd();

    }
}
