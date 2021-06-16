using XIHBasic;
using UnityEngine;
using System;
using UnityEngine.AddressableAssets;
using XiHNet;

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
            try
            {
                await Addressables.LoadSceneAsync("Assets/Bundles/Scenes/LoadScene.unity").Task;//Addressables.InitializeAsync()无需调用，调用也返回invaild op
            }
            catch(Exception e) {
                //一般报错是因为热更资源引用了新的Unity本地资源，需要替换新apk
                //所以为了避免该情况，热更资源应该尽可能只使用热更资源所引用的资源；或设计时多引用本地资源（可能此时用不到，但以后热更可能会引用到）
                Debug.Log(e.Message);
                Caching.ClearCache();
            }
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
