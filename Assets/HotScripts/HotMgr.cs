using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using Aot;
using FairyGUI;
using XiHUI;
using Cysharp.Threading.Tasks;
using XiHUtil;
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
            //如果需要将字体打包到AssetBundle，那么需要自行加载并注册字体
            var font = YooAssets.LoadAssetSync<Font>("Assets/Res/Aot2Hot/Font/JTFont.ttf");
            FontManager.RegisterFont(new DynamicFont("JTFont", font.AssetObject as Font), "JTFont");
            UIPackage.unloadBundleByFGUI = false;
            UIDialogManager.Instance.InitCommonPackage(new List<string>() { "Common"});
            //UIDialogManager.Instance.InitConfig();


            DontDestroyOnLoad(this.gameObject);//包含事件监听组件EventSystem AudioListener
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            await YooAssets.LoadSceneAsync("Assets/Res/HotScene/Home.unity").ToUniTask();
            UIUtil.OpenDialog<HomeDialog>("Home","Home",Mode.Stack);
        }
    }
}
