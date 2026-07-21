using UnityEngine;

namespace Hot
{
    public partial class GameBase
    {
        protected override void GamePause(bool pause)
        {
            base.GamePause(pause);
            if (pause)
            {
                StopTime();
            }
            else {
                ResumeTime();
            }
#if UNITY_GM
            Debug.LogWarning("GameBase GamePause..." + stopTime);
#endif
        }
    }
}
