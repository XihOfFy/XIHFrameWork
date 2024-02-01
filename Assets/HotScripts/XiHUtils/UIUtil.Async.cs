using Cysharp.Threading.Tasks;
using XiHUI;

namespace XiHUtil
{
    public static partial class UIUtil
    {
        public async static UniTask<UIDialog> OpenDialogAsync(string dialogName, string packageName, string componentName, Mode layer = 0, bool isFull = true, bool isBlur = false, params string[] dependencyUIPackages)
        {
            return await UIDialogManager.Instance.OpenAsync(dialogName, packageName, componentName, layer, isFull, isBlur, dependencyUIPackages);
        }

        public async static UniTask<T> OpenDialogAsync<T>(string packageName, string componentName, Mode layer = 0, bool isFull = true, bool isBlur = false, params string[] dependencyUIPackages) where T : UIDialog
        {
            return await OpenDialogAsync(typeof(T).Name, packageName, componentName, layer, isFull, isBlur, dependencyUIPackages) as T;
        }
        /*public async static void LoadScene(string path) {
            (await OpenDialogAsync<SceneChangeDialog>()).Show(path).Forget();
        }*/

        public static UniTask GetDependencyUIPackage(string packageName, UIDialog reference) {
            return UIDialogManager.Instance.GetDependenceUIPackageAsync(packageName,reference);
        }
    }
}
