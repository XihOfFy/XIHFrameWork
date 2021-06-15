using XIHBasic;
using UnityEngine;
using System;
using UnityEngine.AddressableAssets;

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
            //为避免服务端关闭导致无限等待加载，所以Remote Group设置超时10s
            //为了无网络也能加载该场景，所以首次打包时将此场景放入AA的Local CanUpdate Group组；之后修改场景会增量更新到新的Group
            try
            {
                await Addressables.LoadSceneAsync("Assets/Bundles/Scenes/LoadScene.unity").Task;//Addressables.InitializeAsync()无需调用，调用也返回invaild op
            }
            catch(Exception e) {
                Debug.Log(e.Message);
                Caching.ClearCache();
            }
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
