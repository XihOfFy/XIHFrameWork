using System.Collections.Generic;
using UnityEngine;
using XiHUI;
using Cysharp.Threading.Tasks;
using XiHUtil;
using XiHSound;
using Tmpl;

namespace Hot
{
    public class HotMgr : MonoBehaviour
    {

        private void Awake()
        {
            ChannelSDKMgr.Instance.sdkBase.Init(res => {
                InitHot().Forget();
            });
        }
        async UniTaskVoid InitHot() {
            await UIDialogManager.Instance.InitCommonPackageAsync(new List<string>() { "Common"});
            //UIDialogManager.Instance.InitConfig();

            await Tables.LoadAllTmpl();
            SoundMgr.Instance.PlayBGM(1);//初始化音频，并播放一个音乐
            
            DontDestroyOnLoad(this.gameObject);//包含事件监听组件EventSystem AudioListener
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            PlatformUtil.SetFramePerSecond(60);

            await SceneChangeDialog.LoadHomeScene();
        }
    }
}
