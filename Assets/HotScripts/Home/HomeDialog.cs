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

        void OnStarBtn() {
            UIUtil.OpenDialog<ChooseDialog>("Common", "Choose",Mode.Popup).Show("标题","内容",val=> {
                UIUtil.OpenDialog<SystemTipDialog>("Common", "SystemTip", Mode.TopMost).Show($"当前选择: {val}");
            },"确定","取消");
        }
    }
}
