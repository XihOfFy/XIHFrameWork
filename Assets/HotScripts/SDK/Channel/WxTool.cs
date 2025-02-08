using System;
using System.Linq;
using UnityEngine;

#if UNITY_WX
using WeChatWASM;
#endif

namespace Hot
{
    public static class WxTool
    {
        public static readonly string WxCurSDKVersion;
        static WxTool() {
            //通过版本号比较的方式进行运行低版本兼容逻辑。
            //版本号比较适用于所有情况。部分场景下也可以使用对于新增的 API，可以通过判断该API是否存在来判断是否支持用户使用的基础库版本
            WxCurSDKVersion = "1.0.0";
#if UNITY_WX
            //2.25.3
            try
            {
                if (WX.CanIUse("GetAppBaseInfo"))
                {
                    WxCurSDKVersion = WX.GetAppBaseInfo().SDKVersion;
                    Debug.Log($"WxTool 1: {WxCurSDKVersion}");
                }
                else {
                    WxCurSDKVersion = WX.GetSystemInfoSync().SDKVersion;
                    Debug.Log($"WxTool 2: {WxCurSDKVersion}");
                }
            } catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.Message);
                WxCurSDKVersion = WX.GetSystemInfoSync().SDKVersion;
                Debug.Log($"WxTool 3: {WxCurSDKVersion}");
            }
#endif
        }
        /// <summary>
        /// WxCurSDKVersion >= requireVersion? true : false
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool CanUseByVersion(string requireVersion) { 
            var vs1 = WxCurSDKVersion.Split('.').ToList();
            var vs2 = requireVersion.Split('.').ToList();
            var len = Math.Max(vs1.Count, vs2.Count);
            while (vs1.Count < len)
            {
                vs1.Add("0");
            }
            while (vs2.Count < len)
            {
                vs2.Add("0");
            }

            for (var i = 0; i < len; i++)
            {
                int.TryParse(vs1[i], out int num1);

                int.TryParse(vs2[i], out int num2);

                if (num1 > num2)
                {
                    return true;
                }
                else if (num1 < num2)
                {
                    return false;
                }
            }
            return true;
        }

        public static void ReportExpt(string key,int val) {
#if UNITY_WX
            WeChatWASM.WX.ReportEvent(key,val);
#endif
        }
    }
}
