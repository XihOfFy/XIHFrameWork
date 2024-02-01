using FairyGUI;
using XiHUI;
using XiHUtil;

namespace Hot
{
    public class HomeDialog : UIDialog
    {
        GButton startBtn;
        protected override void InitComponent()
        {
            base.InitComponent();
            startBtn.onClick.Add(OnStarBtn);
        }

        async void OnStarBtn() {
            (await UIUtil.OpenDialogAsync<ChooseDialog>("Common", "Choose", Mode.Popup)).Show("标题","内容", async val => {
                UIUtil.ShowSystemTip($"当前选择: {val}");
            },"确定","取消");
        }
    }
}
