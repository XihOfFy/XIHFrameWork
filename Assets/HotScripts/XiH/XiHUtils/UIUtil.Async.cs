using Cysharp.Threading.Tasks;
using Tmpl;
using XiHUI;

namespace XiHUtil
{
    public static partial class UIUtil
    {
        async static UniTask<UIDialog> OpenDialogAsync(UIParam param)
        {
            return await UIDialogManager.Instance.OpenAsync(param);
        }

        public async static UniTask<T> OpenDialogAsync<T>() where T : UIDialog
        {
            var key = typeof(T).Name;
            var parmas = Tables.Instance.TbUIParam.Get(key);
            return await OpenDialogAsync(parmas) as T;
        }
        /*public async static void LoadScene(string path) {
            (await OpenDialogAsync<SceneChangeDialog>()).Show(path).Forget();
        }*/

        public static UniTask GetDependencyUIPackage(string packageName, UIDialog reference) {
            return UIDialogManager.Instance.GetDependenceUIPackageAsync(packageName,reference);
        }
    }
}
