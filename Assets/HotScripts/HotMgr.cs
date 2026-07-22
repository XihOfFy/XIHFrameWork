using System.Collections.Generic;
using UnityEngine;
using XiHUI;
using Cysharp.Threading.Tasks;
using XiHSound;
using Tmpl;
using Aot;
using YooAsset;
using Aot.XiHUtil;
using Ad;
using XiHUtil;
using FairyGUI;
using DG.Tweening;
using XiHAsset;

namespace Hot
{
    public partial class HotMgr : MonoBehaviour
    {
        private void Awake()
        {
#if UNITY_WX
            WeChatWASM.WX.ReportScene(new WeChatWASM.ReportSceneOption() { sceneId = 7 });
#elif UNITY_DY
            var param = new TTSDK.UNBridgeLib.LitJson.JsonData();
            param["sceneId"] = 7001;
            param["costTime"] = 100;
            TTSDK.TT.ReportScene(param);
#endif
            DontDestroyOnLoad(this.gameObject);//包含事件监听组件EventSystem AudioListener
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            UIObjectFactory.SetLoaderExtension(typeof(XIHLoader));
            //PlatformUtil.SetFramePerSecond(60);根据游戏需要确定是否锁帧
            //UnityEngine.Input.multiTouchEnabled = false;//禁用多点触屏,对于EvenetSystem的触控没法限制
            DOTween.SetTweensCapacity(1250, 500);
#if UNITY_WEBGL && !UNITY_EDITOR
            //酌情考虑是否开启输入，对于小游戏，开启这个会有一定性能消耗
            WebGLInput.mobileKeyboardSupport = true;
#endif
            InitHot().Forget();

            var pkg = YooAssets.GetPackage(AotConfig.PACKAGE_NAME);
            // 注意：下载完成之后再保存本地版本
            AotPlayerPrefsUtil.Set(AotPlayerPrefsUtil.GAME_RES_VERSION, pkg.GetPackageVersion());
            pkg.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
            pkg.ClearCacheFilesAsync(EFileClearMode.ClearUnusedManifestFiles);
        }
        async UniTaskVoid InitHot()
        {
            await Tables.InitTmpl();
            var tks = new List<UniTask>(3);
            tks.Add(InitThridSdk());
            tks.Add(AssetPoolMgr.Instance.InitPool(""));
            tks.Add(UIDialogManager.Instance.InitCommonPackageAsync(new List<string>() { "Common" }));
            await UniTask.WhenAll(tks);
            SoundMgr.Instance.PlayBGM(1);//初始化音频，并播放一个音乐
            CheckGuide();
        }
    }
}
