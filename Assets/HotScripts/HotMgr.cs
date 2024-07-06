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
            DontDestroyOnLoad(this.gameObject);//包含事件监听组件EventSystem AudioListener
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            PlatformUtil.SetFramePerSecond(60);

            ChannelSDKMgr.sdkBase.Init(res => {
                InitHot().Forget();
            });
        }
        async UniTaskVoid InitHot() {
            await Tables.LoadAllTmpl();//初始化配置,放在第一
            await UIDialogManager.Instance.InitCommonPackageAsync(new List<string>() { "Common"});//持久化的UI包
            SoundMgr.Instance.PlayBGM(1);//初始化音频，并播放一个音乐
            await SceneChangeDialog.LoadHomeScene();
        }
    }
}
