using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using YooAsset;
using Aot;
using System.Text;
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
            /*
                        FileUtil.WriteFile("frontConfig.txt",JsonUtility.ToJson(AotConfig.frontConfig));
                        var readStr = FileUtil.ReadFile("frontConfig.txt");
                        Debug.LogWarning(readStr);
                        var bys = UTF8Encoding.UTF8.GetBytes(readStr);
                        FileUtil.WriteFile("frontConfig.bs", bys);
                        var rbs = FileUtil.ReadFileBytes("frontConfig.bs");
                        Debug.LogWarning($"{bys.Length} >> {rbs.Length}");
                        var str = UTF8Encoding.UTF8.GetString(rbs);
                        Debug.LogWarning($"{str}");
                        var jb = JsonUtility.FromJson<FrontConfig>(str);
                        Debug.Log(jb.fallbackHostServer);


                        PlayerPrefsUtil.Set("S","s");
                        PlayerPrefsUtil.Set("I",1);
                        PlayerPrefsUtil.Set("F",2.2f);
                        Debug.LogWarning(PlayerPrefsUtil.Get("S", "ns"));
                        Debug.LogWarning(PlayerPrefsUtil.HasKey("I"));
                        Debug.LogWarning(PlayerPrefsUtil.Get("F",0.5f));
                        Debug.LogWarning(PlayerPrefsUtil.Get("I", 0));
                        PlayerPrefsUtil.DeleteKey("I");
                        Debug.LogWarning(PlayerPrefsUtil.HasKey("I"));
                        PlayerPrefsUtil.DeleteAllKey();
                        Debug.LogWarning(PlayerPrefsUtil.HasKey("S"));*/
        }
    }
}
