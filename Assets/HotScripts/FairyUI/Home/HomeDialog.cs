using Cysharp.Threading.Tasks;
using FairyGUI;
using XiHSound;
using XiHUI;
using XiHUtil;

namespace Hot
{
    public class HomeDialog : UIDialog
    {
        GButton setBtn;
        GButton startBtn;
        protected override void InitComponent()
        {
            startBtn.onClick.Add(OnStarBtn);
            setBtn.onClick.Add(OnSetBtn);
        }
        async void OnStarBtn()
        {
            SoundMgr.Instance.PlaySound(10001);
            (await UIUtil.OpenDialogAsync<ChooseDialog>()).Show("标题", "内容", val =>
            {
                UIUtil.ShowSystemTip($"当前选择: {val}");
            }, "确定", "取消");
        }
        void OnSetBtn()
        {
            SoundMgr.Instance.PlaySound(10001);
            UIUtil.OpenDialogAsync<BattleSettingDialog>().Forget();
        }
    }
}
