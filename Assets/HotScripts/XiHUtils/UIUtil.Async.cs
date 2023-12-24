using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using XiHUI;

namespace XiHUtil
{
    public static partial class UIUtil
    {
        public async static UniTask<UIDialog> OpenDialogAsync(string dialogName, string packageName, string compName, Mode mode = Mode.TopMost, bool isFull = true, bool isBlur = false)
        {
            return await UIDialogManager.Instance.OpenAsync(dialogName, packageName, compName, mode, isFull, isBlur);
        }

        public async static UniTask<T> OpenDialogAsync<T>(string packageName, string compName, Mode mode = Mode.TopMost, bool isFull = true, bool isBlur = false) where T : UIDialog
        {
            return await OpenDialogAsync(typeof(T).Name, packageName, compName, mode, isFull, isBlur) as T;
        }
    }
}
