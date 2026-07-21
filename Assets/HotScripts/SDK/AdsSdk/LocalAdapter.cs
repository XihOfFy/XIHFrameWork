
using Cysharp.Threading.Tasks;
using Hot;
using System;

namespace Ad
{
    public class LocalAdapter : IAdAdapter
    {
        public void ShowRewardAdv(Action<bool> onLoad, VideoSceneEnum comment, int pLevel = 0, int pProcess = 0)
        {
            DelayInsert(300, callback: () =>
            {
                IChannelAdapter.GamePause?.Invoke(true);
                DelayInsert(300, callback: () =>
                {
                    IChannelAdapter.GamePause?.Invoke(false);
                    onLoad?.Invoke(true);
                }).Forget();
            }).Forget();
        }

        public void InitCallBack()
        {

        }
        public void InitSDK()
        {

        }

        public bool IsHaveReadyAd()
        {
            return true;
        }

        public void LoadVideo(Action<bool> onLoad)
        {

        }

        public void RemoveCallBack()
        {

        }


        public void CloseNativeAd()
        {
        }

        public void ShowInsertAdv(Action<bool> onLoad, VideoSceneEnum comment)
        {
            DelayInsert(callback: () =>
            {
                onLoad?.Invoke(true);
            }).Forget();
        }

        async UniTaskVoid DelayInsert(int ms = 1000, Action callback = null)
        {
            await UniTask.Delay(ms);
            callback?.Invoke();
        }
    }
}
