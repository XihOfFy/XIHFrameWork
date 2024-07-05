using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using Aot;
using FairyGUI;
using XiHUI;
using Cysharp.Threading.Tasks;
using XiHUtil;
using TMPro;
using XiHSound;
using Tmpl;
using SimpleJSON;

#if UNITY_WX
using WeChatWASM;
#endif
namespace Hot
{
    public class HotMgr : MonoBehaviour
    {

        private void Awake()
        {
            YooAssets.GetPackage(AotConfig.PACKAGE_NAME).UnloadUnusedAssets();

#if UNITY_WX
            WX.InitSDK(_ => {
                Debug.Log($" WX.InitSDK:{_}");
                InitHot().Forget();
            });
#else
            InitHot().Forget();
#endif
        }
        async UniTaskVoid InitHot() {
            await UIDialogManager.Instance.InitCommonPackageAsync(new List<string>() { "Common"});
            //UIDialogManager.Instance.InitConfig();

            await Tables.LoadAllTmpl();
            _ = SoundMgr.Instance;//初始化音频

            DontDestroyOnLoad(this.gameObject);//包含事件监听组件EventSystem AudioListener
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            PlatformUtil.SetFramePerSecond(60);

            await SceneChangeDialog.LoadHomeScene();
        }
    }
}
