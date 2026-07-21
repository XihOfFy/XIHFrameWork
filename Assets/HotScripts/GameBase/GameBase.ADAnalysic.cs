using UnityEngine;

namespace Hot
{
    public partial class GameBase
    {
        [HideInInspector] public int adCnt;//广告总次数，无论何总
        public virtual void AdSucessEnd() {
            ++adCnt;
        }
    }
}
