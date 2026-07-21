#if UNITY_WX
using WeChatWASM;
#endif

using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Hot
{
    public static partial class WxTool
    {
        public static async UniTask<bool> CheckIsAddedToMyMiniProgram()
        {
#if UNITY_WX
            var end = false;
            var result = false;
            WeChatWASM.WX.CheckIsAddedToMyMiniProgram(new CheckIsAddedToMyMiniProgramOption()
            {
                complete = res => end = true,
                success = res => result = res.added,
                fail = res => Debug.LogError(res.errMsg),
            }) ;
            await UniTask.WaitUntil(() => end);
            return result;
#else 
            return true;
#endif
        }
    }
}
