using UnityEngine;
using XiHUtil;

namespace Hot
{
    public class EnvCheck
    {
        public static bool IsDevEnv() {
#if UNITY_WX
            var res =  WeChatWASM.WX.GetAccountInfoSync();
            //- 'develop': 	开发版，提交代码审核时默认使用开发版进行审核; - 'trial': 体验版; - 'release': 正式版;
            var env = res.miniProgram.envVersion;
            Debug.Log("Env:"+ env);
            return ("develop".Equals(env) || "trial".Equals(env)) && WeChatWASM.WX.GetAppBaseInfo().enableDebug;
            //return "trial".Equals(env) && WeChatWASM.WX.GetAppBaseInfo().enableDebug;
#elif (UNITY_DY||UNITY_TT)
            if (TTSDK.TT.s_ContainerEnv != null) {
                var ttType = TTSDK.TT.s_ContainerEnv.GetVersionType();
//#if UNITY_DY
//                    TTSDK.TT.EnableTTSDKDebugToast && 
//#endif
                return (ttType == TTSDK.VersionType.Test || ttType == TTSDK.VersionType.Perview);
            }
#elif UNITY_ANDROID
            var fullPath = FileUtil.RootPath + "/yddev.cfg";
            return FileUtil.FileExist(fullPath);
#elif UNITY_IOS
            var version = Application.version;
            return "999.999.999".Equals(version);
#else

#endif
            return false;
        }
    }
}
