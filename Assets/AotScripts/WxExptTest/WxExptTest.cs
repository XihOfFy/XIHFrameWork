#if UNITY_WX
using WeChatWASM;
using UnityEngine;

namespace Aot
{
    public class WxExptTest
    {
        public static string GetExptGroup()
        {
            var test = "wxapp_expt_test";
            var dev = "expt_dev_test";
            var res = WX.GetExptInfoSync(new string[] { test, dev });
            if (res == null) return "";
            if (res.TryGetValue(nameof(dev),out var val) && val.Equals("999")) return "Dev";//优先开发者测试
            if (res.TryGetValue(nameof(test), out val)) return val;
            return "";
        }
    }
}
#endif