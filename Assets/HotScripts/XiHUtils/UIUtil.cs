using Hot;
using System.Collections.Generic;
using XiHUI;

namespace XiHUtil
{
    public static partial class UIUtil
    {
        /*public static UIDialog OpenDialog(string dialogName, string packageName, string compName, Mode mode = Mode.TopMost, bool isFull = true, bool isBlur = false)
        {
            return UIDialogManager.Instance.Open(dialogName, packageName, compName, mode, isFull, isBlur);
        }

        public static T OpenDialog<T>(string packageName, string compName, Mode mode = Mode.TopMost, bool isFull = true, bool isBlur = false) where T : UIDialog
        {
            return OpenDialog(typeof(T).Name, packageName, compName, mode, isFull, isBlur) as T;
        }*/

        public static void CloseDialog<T>() where T : UIDialog
        {
            CloseDialog(typeof(T).Name);
        }

        public static void CloseDialog(string dialogName)
        {
            UIDialogManager.Instance.Close(dialogName);
        }

        public static T GetDialog<T>() where T : UIDialog
        {
            return GetDialog(typeof(T).Name) as T;
        }

        public static UIDialog GetDialog(string dialogName)
        {
            return UIDialogManager.Instance.GetDialog(dialogName);
        }

        public static bool IsDialogInStack(string dialogName)
        {
            return UIDialogManager.Instance.IsDialogOpen(dialogName);
        }

        public static bool IsDialogOpen<T>() where T : UIDialog
        {
            return IsDialogOpen(typeof(T).Name);
        }

        public static bool IsDialogOpen(string dialogName)
        {
            return UIDialogManager.Instance.GetDialog(dialogName)?.State == State.Open;
        }

        /// <summary>
        /// 关闭指定列表之外的全部界面
        /// </summary>
        /// <param name="exceptNames">不关闭的界面列表</param>
        public static void CloseAll(HashSet<string> exceptNames = null)
        {
            UIDialogManager.Instance.CloseAll(exceptNames);
        }
        /// <summary>
        /// 关闭弹框界面
        /// </summary>
        /// <param name="exceptNames"></param>
        public static void CloseAllPopUI(HashSet<string> exceptNames = null)
        {
            UIDialogManager.Instance.CloseAllPopUI(exceptNames);
        }
        public static async void ShowSystemTip(string tip)
        {
            //提示内容
            (await OpenDialogAsync<SystemTipDialog>("Common", "SystemTip", Mode.TopMost)).Show(tip);
        }
    }
}
