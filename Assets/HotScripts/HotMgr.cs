using System.Collections.Generic;
using UnityEngine;
using XiHUI;
using Cysharp.Threading.Tasks;
using XiHSound;
using Tmpl;
using Aot;
using YooAsset;
using Aot.XiHUtil;
using WeChatWASM;
using Ad;

namespace Hot
{
    public class HotMgr : MonoBehaviour
    {
        private void Awake()
        {

            DontDestroyOnLoad(this.gameObject);//包含事件监听组件EventSystem AudioListener
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            //PlatformUtil.SetFramePerSecond(60);根据游戏需要确定是否锁帧

            var pkg = YooAssets.GetPackage(AotConfig.PACKAGE_NAME);
            // 注意：下载完成之后再保存本地版本
            AotPlayerPrefsUtil.Set(AotPlayerPrefsUtil.GAME_RES_VERSION, pkg.GetPackageVersion());
            pkg.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
            pkg.ClearCacheFilesAsync(EFileClearMode.ClearUnusedManifestFiles);

            AdSdkMgr.sdkBase.InitSDK();//若使用广告，可以提前初始化
            ChannelSDKMgr.sdkBase.Init(res => {
                InitHot().Forget();
            });
        }
        async UniTaskVoid InitHot() {
            InitShaderVariants().Forget();//看情况是否预加载编译变体
            var tks = new List<UniTask>();
            tks.Add(Tables.LoadAllTmpl());//初始化配置,放在第一
            tks.Add(UIDialogManager.Instance.InitCommonPackageAsync(new List<string>() { "Common" }));//持久化的UI包
            await UniTask.WhenAll(tks);
            SoundMgr.Instance.PlayBGM(1);//初始化音频，并播放一个音乐
            ChannelSDKMgr.sdkBase.TouchOverride(this.gameObject);//处理小游戏平台触屏粘连，例如：摄像机射线监测点击物体，触发多次点击事件
            await SceneChangeDialog.LoadHomeScene();


#if UNITY_WX
            if (WxTool.CanUseByVersion("2.26.2"))
            {
                WX.ReportScene(new ReportSceneOption() { sceneId = 7 });
            }
            else
            {
                Debug.LogWarning("暂时不支持该WX API使用：WX.ReportScene");
            }
#elif UNITY_DY
            var param = new TTSDK.UNBridgeLib.LitJson.JsonData();
            param["sceneId"] = 7001;
            param["costTime"] = 100;
            TTSDK.TT.ReportScene(param);
#endif
            //UnityEngine.Input.multiTouchEnabled = false;//禁用多点触屏,对于EvenetSystem的触控没法限制
            //DOTween.SetTweensCapacity(1250, 500);
#if UNITY_WEBGL &&!UNITY_DY && !UNITY_WX && !UNITY_EDITOR && (UNITY_2021_2_5 || UNITY_2022_1_OR_NEWER)
            //酌情考虑是否开启输入，对于小游戏，开启这个会有一定性能消耗
            WebGLInput.mobileKeyboardSupport = true;
#endif

        }
        async UniTaskVoid InitShaderVariants() {
            var handle = YooAssets.LoadAssetAsync<ShaderVariantCollection>("Assets/Res/ShaderVariants/ShaderVariants.shadervariants");
            await handle;
            var asset = handle.GetAssetObject<ShaderVariantCollection>();
            asset.WarmUp();
            handle.Release();
        }
    }
}
