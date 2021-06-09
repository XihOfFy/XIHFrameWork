using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using XIHBasic;
namespace XIHHotFix
{
    public class LoginSceneMgr : AbsComponent
    {
        public LoginSceneMgr(MonoDotBase dot) : base(dot) { }
        private Button enter;
        protected override void Awake()
        {
            enter = MonoDot.GameObjsDic["Enter"].GetComponent<Button>();
            bool isLoading = false;
            enter.onClick.AddListener(async () =>
            {
                if (isLoading) return;
                isLoading = true;
                await Addressables.LoadSceneAsync("Assets/Bundles/Scenes/BattleScene.unity").Task;
            });
        }

        protected override void OnDestory()
        {
        }

        protected override void OnDisable()
        {
        }

        protected override void OnEnable()
        {
        }
    }
}
