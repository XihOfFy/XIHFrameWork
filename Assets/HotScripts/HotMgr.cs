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
            //如果需要将字体打包到AssetBundle，那么需要自行加载并注册字体
            var font = YooAssets.LoadAssetAsync<Font>("Assets/Res/Aot2Hot/Font/JTFont.ttf");
            await font.ToUniTask();
            FontManager.RegisterFont(new DynamicFont("JTFont", font.AssetObject as Font), "JTFont");
/*
            //tmp pro 字体 有点糊，先用上面字体
            var font = YooAssets.LoadAssetSync<TMP_FontAsset>("Assets/Res/Aot2Hot/Font/JTFont.asset");
            FontManager.RegisterFont(new TMPFont() { fontAsset = font.AssetObject as TMP_FontAsset,name= "JTFont" });
*/
            UIConfig.defaultFont = "JTFont"; //另一个方法是FGUI项目里添加jtfont字体，直接引用，发布时字体不会发布，而是找该字体的注册 https://www.fairygui.com/docs/editor/font
            UIPackage.unloadBundleByFGUI = false;
            await UIDialogManager.Instance.InitCommonPackageAsync(new List<string>() { "Common"});
            //UIDialogManager.Instance.InitConfig();

            await Tables.LoadAllTmpl();
            var datat = Tables.Instance.TbUIParam.DataList;
            foreach (var data in datat) Debug.Log(data);

            DontDestroyOnLoad(this.gameObject);//包含事件监听组件EventSystem AudioListener
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            await YooAssets.LoadSceneAsync("Assets/Res/HotScene/Home.unity").ToUniTask();
            await UIUtil.OpenDialogAsync<HomeDialog>();

            _ = SoundMgr.Instance;//初始化
        }
    }
}
