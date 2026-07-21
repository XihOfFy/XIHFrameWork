namespace XiHSound
{
    public partial class SoundMgr
    {
        public void PlayMainBGM() {
            PlayBGM(0);
        }
        public void PlayGameBGM(int bgId) {
            PlayBGM(bgId);
        }
        /// <summary>
        /// 游戏内所有按钮点击,包括开始、菜单，关闭、回退等
        /// </summary>
        public void PlayBtnSound() { 
            PlaySound(1001);
        }
        public void PlayCloseSound() { 
            PlaySound(1002);
        }
    }
}
