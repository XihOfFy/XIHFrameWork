using Cysharp.Threading.Tasks;
using XiHAsset;

namespace Hot
{
    public abstract partial class GameBase : XiHAssetBaseMgr<GameBase> {
        public abstract UniTask InitGame<C>(C cfg) where C : Luban.BeanBase;
    }
    public abstract partial class GameBase<T,D,S> : GameBase where T : Luban.BeanBase where D: AbsDataSave<D>, new() where S : GameBase<T, D, S>
    {
        public static S InstanceMini;
        public T stageCfg;
        public D GameDataSave => AbsDataSave<D>.Instance;
        protected override void SetInstance()
        {
            base.SetInstance();
            InstanceMini = (S)this;
        }
        protected override void DestoryInstance()
        {
            base.DestoryInstance();
            InstanceMini = null;
        }
        public override async UniTask InitGame<C>(C cfg)
        {
            GameDataSave.InitStartGame();
            this.stageCfg = cfg as T;
            await StartGame();
        }
    }
}
