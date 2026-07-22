using FairyGUI;
using Tmpl;
using XiHSound;
using XiHUtil;

namespace Hot
{
    public class BattleSettingDialog : SettingBaseDialog
    {
        //GButton homeBtn;
        //GButton againBtn;
        protected override void InitComponent()
        {
            base.InitComponent();
            //GameBase.Instance.StopTime();

            //againBtn.title = 7005.Translate();
            //homeBtn.title = 7006.Translate();
            //homeBtn.onClick.Add(OnHomeBtnAsync);
            //againBtn.onClick.Add(OnRestartBtn);
        }
        public override void OnGMBtn()
        {
#if USE_GM
            GameBase.Instance.isGmShow = !GameBase.Instance.isGmShow;
#else
            var isGmEnv = EnvCheck.IsDevEnv();
            if (isGmEnv)
            {
                GameBase.Instance.isGmShow = !GameBase.Instance.isGmShow;
            }
#endif
        }
    }
}