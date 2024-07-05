using Cysharp.Threading.Tasks;
using XiHUtil;

namespace Hot
{
    public partial class SceneChangeDialog
    {
        public static async UniTask LoadHomeScene() {
            var sceneDialog = await UIUtil.OpenDialogAsync<SceneChangeDialog>();
            await sceneDialog.Show<HomeDialog>("Assets/Res/HotScene/Home.unity");
        }
    }
}
