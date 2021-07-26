using XIHBasic;
using UnityEngine;
using System;
using UnityEngine.AddressableAssets;
using XiHNet;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace XIHHotFix {
    public static class HotFixInit
    {
        public static event Action Update;
        public static event Action FixedUpdate;
        public static Action<bool> OnApplicationFocus { get; set; }
        public static Action<bool> OnApplicationPause { get; set; }
        static void Init() {
            HotFixBridge.Update = XIHUpdate;
            HotFixBridge.FixedUpdate = XIHFixedUpdate;
            HotFixBridge.OnApplicationFocus = (_)=> OnApplicationFocus?.Invoke(_);
            HotFixBridge.OnApplicationPause = (_) => OnApplicationPause?.Invoke(_);
            Debug.Log("HotFixInit.Init");
            IMessageExt.Init();
            LoadScene();
        }
        static async void LoadScene() {
            //为了无网络也能加载该场景，所以首次打包时将此场景放入AA的Local CanUpdate Group组；之后修改场景会增量更新到新的Group
            //Addressables.InitializeAsync()无需调用，调用也返回invaild op
            var handle = Addressables.LoadSceneAsync(PathConfig.AA_Scene_Load).Task;//首次调用自动将会检测hash更新，内部会串行任务，先执行init再执行此任务
            bool pass = false;
            async void DoWait()
            {
                await Task.Delay(5000);
                if (pass) return;
                if (handle.Status != TaskStatus.RanToCompletion)
                {
                    //此处报错一般是因为热更资源引用了新的Unity本地资源(需要替换新apk) 或 AA的InitializeAsync()连接目标服务器无响应导致初始化可能失败（Web关服），
                    //所以为了避免该情况，热更资源应该尽可能只使用热更资源所引用的资源；或设计时多引用本地资源（可能此时用不到，但以后热更可能会引用到）
                    PathConfig.ClearAll();
                    //Application.Quit();暴力退出不太好
                    var obj = GameObject.Find("Canvas/ErrorTip");
                    if (obj != null)
                    {
                        obj.GetComponent<Text>().text = "无法进入场景，请下载最新版本或尝试重新运行游戏，并确保网络正常";
                    }
                }
            }
            DoWait();
            await handle;//这个报错是无法捕获异常的，因为属于其他异步Task中,且报错后此await处于漫长等待...若非替换APK的情况，则能加载成功！只是时间长短问题
            pass = true;
        }
        static void XIHUpdate()
        {
            Update?.Invoke();
        }

        static void XIHFixedUpdate()
        {
            FixedUpdate?.Invoke();
        }
    }
}
