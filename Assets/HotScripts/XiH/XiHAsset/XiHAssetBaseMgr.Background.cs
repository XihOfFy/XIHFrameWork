using Hot;
using UnityEngine;

namespace XiHAsset
{
    public partial class XiHAssetBaseMgr
    {
        [HideInInspector] public bool gamePaused;
        protected void AddLifeCycle()
        {
            gamePaused = false;
            ChannelSDKMgr.GamePause = GamePause;
        }
        protected void RemoveLifeCycle()
        {
            ChannelSDKMgr.GamePause = null;
        }
        protected virtual void GamePause(bool pause)
        {
            gamePaused = pause;
        }
    }
}
