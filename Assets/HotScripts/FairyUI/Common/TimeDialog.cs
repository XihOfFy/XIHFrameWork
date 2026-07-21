using XiHUI;
namespace Hot
{
    public class TimeDialog : UIDialog
    {
        protected override void InitComponent()
        {
            ChannelSDKMgr.GamePause?.Invoke(true);
            //if (GameBase.Instance) GameBase.Instance.StopTime();
        }
        protected override void OnClose()
        {
            ChannelSDKMgr.GamePause?.Invoke(false);
            //if (GameBase.Instance) GameBase.Instance.ResumeTime();
        }
    }
}
