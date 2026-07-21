using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Hot
{
    public partial class GameBase
    {
        [HideInInspector] public bool gameStart;
        [HideInInspector] public bool gameEnd;
        [HideInInspector] public int workingCount;//工作数量 workingCount==0表示稳定，例如针对异步，可能结算要等待稳定，或返回主界面存档类游戏也需要稳定后才能执行存档
        protected abstract UniTask StartGame();
        public abstract bool CheckGameWin();
        protected abstract UniTaskVoid Win();
        protected abstract UniTaskVoid Fail();
        public abstract void RebirthGame();
    }
}
