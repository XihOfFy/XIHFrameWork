using XIHBasic;
using UnityEngine;
using System;
using UnityEngine.AddressableAssets;
using XiHNet;
using System.IO;
using System.Threading.Tasks;

namespace XIHHotFix {
    public static class HotFixInit
    {
        public static event Action Update;
        public static event Action FixedUpdate;
        public static Action<bool> OnApplicationFocus { get; set; }
        public static Action<bool> OnApplicationPause { get; set; }
        static async void Init() {
            HotFixBridge.Update = XIHUpdate;
            HotFixBridge.FixedUpdate = XIHFixedUpdate;
            HotFixBridge.OnApplicationFocus = (_)=> OnApplicationFocus?.Invoke(_);
            HotFixBridge.OnApplicationPause = (_) => OnApplicationPause?.Invoke(_);
            Debug.Log("HotFixInit.Init");
            //为了无网络也能加载该场景，所以首次打包时将此场景放入AA的Local CanUpdate Group组；之后修改场景会增量更新到新的Group
            //Addressables.InitializeAsync()无需调用，调用也返回invaild op
            var handle = Addressables.LoadSceneAsync(PathConfig.AA_Scene_Load).Task;
            async void DoWait() {
                await Task.Factory.StartNew(async () => {
                    await Task.Delay(5000);
                    if (handle.Status != TaskStatus.RanToCompletion)
                    {
                        //一般报错是因为热更资源引用了新的Unity本地资源，需要替换新apk
                        //所以为了避免该情况，热更资源应该尽可能只使用热更资源所引用的资源；或设计时多引用本地资源（可能此时用不到，但以后热更可能会引用到）
                        PathConfig.ClearAll();
                        Application.Quit();
                    }
                });
            }
            DoWait();
            await handle;//这个报错是无法捕获异常的，因为属于其他异步Task中,且报错后此await处于永远等待...
            IMessageExt.Init();
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
