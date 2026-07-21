using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Hot
{
    public partial class GameBase
    {
        [HideInInspector]public float playTime;//游戏时间（不含暂停时间） 都是有效时间
        [HideInInspector]public int stopTime;
        [HideInInspector]public float gameTime;
        public abstract UniTaskVoid GameTimeOver();
        public void ReviveTime(int timeSec) {
            gameEnd = false;
            gameTime += timeSec;
        }
        public void StopTime()
        {
            stopTime += 1;
        }
        public void ResumeTime() {
            stopTime -= 1;
            if (stopTime < 0)
            {
                Debug.LogError($"ResumeTime {stopTime} less 0");
                stopTime = 0;
            }
        }
    }
}
