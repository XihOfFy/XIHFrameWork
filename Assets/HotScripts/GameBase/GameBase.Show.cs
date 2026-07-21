using UnityEngine;

namespace Hot
{
    public partial class GameBase
    {
        [HideInInspector] public int totalCount;
        [HideInInspector] public int leftCount;
        public int GetProgress() {
            if (totalCount <= 0) return 0;
            return 100 * (totalCount - leftCount) / totalCount;
        }
 
        public void DecreaseLeftCount(int cnt) {
            leftCount -= cnt;
            ShowUI();
        }
        protected abstract void ShowUI();
    }
}
