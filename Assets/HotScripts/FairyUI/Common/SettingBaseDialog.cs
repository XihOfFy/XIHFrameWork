using Aot;
using Aot2Hot;
using FairyGUI;
using Tmpl;
using XiHSound;
using XiHUI;

namespace Hot
{
    /// <summary>
    /// 设置UI界面，包含音乐、音效、振动、设置及版本信息
    /// </summary>
    public abstract class SettingBaseDialog : TimeDialog
    {
        GButton closeBtn;
        GTextField musicTxt;
        GButton musicBtn;
        GTextField soundTxt;
        GButton soundBtn;
        GTextField vibrateTxt;
        GButton vibrateBtn;
        GLabel labTitle;
        GTextField verTxt;

        protected override void InitComponent()
        {
            base.InitComponent();
            closeBtn.onClick.Add(OnCloseBtn);
#if USE_YOO
            verTxt.SetText($"{AotConfig.Version}({YooAsset.YooAssets.GetPackage(AotConfig.PACKAGE_NAME).GetPackageVersion()})");
#else
            verTxt.SetText(AotConfig.Version);
#endif
            labTitle.GetTextField().Translate(790001);
            labTitle.onClick.Add(OnGMBtn);
            musicTxt.Translate(790002);
            musicBtn.onClick.Add(OnMusicBtn);
            musicBtn.selected = SoundMgr.Instance.BgmEnable;
            soundTxt.Translate(790003);
            soundBtn.onClick.Add(OnSoundBtn);
            soundBtn.selected = SoundMgr.Instance.SoundEnable;
            vibrateTxt.Translate(790004);
            vibrateBtn.onClick.Add(OnVibrateBtn);
            vibrateBtn.selected = SoundMgr.Instance.Vibrate;
        }
        public abstract void OnGMBtn();

        void OnMusicBtn()
        {
            SoundMgr.Instance.BgmEnable = musicBtn.selected;
            SoundMgr.Instance.PlayBtnSound();
        }
        void OnSoundBtn()
        {
            SoundMgr.Instance.SoundEnable = soundBtn.selected;
            SoundMgr.Instance.PlayBtnSound();
        }
        void OnVibrateBtn()
        {
            if (SoundMgr.Instance.Vibrate = vibrateBtn.selected)
            {
                SoundMgr.Instance.PlayVibrate();
            }
            SoundMgr.Instance.PlayBtnSound();
        }

        protected void OnCloseBtn()
        {
            SoundMgr.Instance.PlayCloseSound();
            Close();
        }
    }
}
