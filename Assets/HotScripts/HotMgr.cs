using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using YooAsset;
using Aot;
using System.Text;
using FairyGUI;
#if UNITY_WX
using WeChatWASM;
#endif
namespace Hot
{
    public class HotMgr : MonoBehaviour
    {
        public TMP_Text tip;

        private void Awake()
        {
            tip.text = "这里是热更场景111";
            YooAssets.GetPackage(AotConfig.PACKAGE_NAME).UnloadUnusedAssets();

#if UNITY_WX
            WX.InitSDK(_ => {
                Debug.Log($" WX.InitSDK:{_}");
                InitHot();
            });
#else
            InitHot();
#endif
        }
        void InitHot() {
            tip.text = "初始化";
            //如果需要将字体打包到AssetBundle，那么需要自行加载并注册字体
            var font = YooAssets.LoadAssetSync<Font>("Assets/Res/Aot2Hot/Font/JTFont.ttf");
            FontManager.RegisterFont(new DynamicFont("JTFont", font.AssetObject as Font), "JTFont");
            UIPackage.unloadBundleByFGUI = false;
        }
    }
}
