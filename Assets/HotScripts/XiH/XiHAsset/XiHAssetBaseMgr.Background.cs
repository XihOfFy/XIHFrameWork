using UnityEngine;

namespace XiHAsset
{
    public partial class XiHAssetBaseMgr
    {
        [HideInInspector] public bool gamePaused;
        protected void AddLifeCycle()
        {
            gamePaused = false;
            IChannelAdapter.GamePause = GamePause;
        }
        protected void RemoveLifeCycle()
        {
            IChannelAdapter.GamePause = null;
        }
        protected virtual void GamePause(bool pause)
        {
            gamePaused = pause;
        }
    }
}
