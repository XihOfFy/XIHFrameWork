using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using System.Threading;

#if USE_YOO
using YooAsset;
#endif
namespace Aot.XiHUtil
{
    public class AssetRef {
        public int refCount;
        float lastuseTime;
        public bool CanRelease(float curTime) => refCount==0 && curTime - lastuseTime > AssetLoadUtil.RELEASE_TIME;
        public void Release()
        {
            refCount -= 1;
            lastuseTime = Time.realtimeSinceStartup;
        }
        public AssetRef Retain() {
            refCount += 1;
            return this;
        }
#if USE_YOO
        public AssetHandle assetHandle;
        public bool IsDone => assetHandle.IsDone;
        public bool IsValid => assetHandle.IsValid;
        public AssetRef(AssetHandle assetHandle) {
            this.assetHandle = assetHandle;
        }
        public T GetAsset<T>() where T : Object
        {
            return assetHandle.AssetObject as T;
        }
        public UniTask ToUniTask(CancellationToken cancellationToken = default)
        {
            return assetHandle.ToUniTask(cancellationToken: cancellationToken);
        }

        /*若要断线重连，可以使用这个方式
         * 若之前已经package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);那么可能内部DCFSLoadAssetBundleOperation会无限重连处理（边玩边下下载器引用计数没有Release），所以这里永远不会报错404，也就是没必要try，等网络恢复它将自动完成
         * public async UniTask ToUniTask(CancellationToken cancellationToken = default)
        {
            try
            {
                //这里可能因为yoo网络错误导致失败，所以try下做后续处理，若使用了后台下载，这里可能会无限等待，因为后台下载会无限次请求直到完成
                await assetHandle.ToUniTask(cancellationToken: cancellationToken);
            }
            catch
            {
                if (assetHandle.LastError.StartsWith("HTTP"))//yoo 网络错误是  Error = HTTP/1.1 404 Not Found
                {
                    await ToUniTask(int.MaxValue);
                }
                else
                {
                    await ToUniTask(1);
                }
            }
        }
        async UniTask ToUniTask(int leftTimes)
        {
            if (leftTimes < 0) return;
            await UniTask.Yield();
            var info = assetHandle.GetAssetInfo();
            Debug.LogError($"加载{info.AssetPath}错误，剩余尝试{leftTimes}次  {assetHandle.LastError}");//yoo 网络错误是  Error = HTTP/1.1 404 Not Found
            try
            {
                assetHandle.Release();
                YooAssets.GetPackage(Aot.AotConfig.PACKAGE_NAME).TryUnloadUnusedAsset(info);//这个是关键，移除_providers，不然无法重新下载请求,provider.Status为上次的,所以一直失败不重新拉取
                assetHandle = YooAssets.LoadAssetAsync(info.AssetPath, info.AssetType);
                await assetHandle.ToUniTask();
            }
            catch
            {
                await ToUniTask(--leftTimes);
            }
        }*/

        public GameObject InstantiateSync(Transform transform = null)
        {
            return assetHandle.InstantiateSync(transform);
        }
        internal void RealRelease()
        {
            assetHandle.Release();
        }
#else
        public Object assetObj;
        public bool IsDone => true;
        public bool IsValid=> assetObj != null;
        public AssetRef(Object assetObj)
        {
            this.assetObj = assetObj;
        }
        public T GetAsset<T>() where T: Object {
            return assetObj as T;
        }
        public UniTask ToUniTask(CancellationToken cancellationToken = default)
        {
            return UniTask.CompletedTask;
        }
        public GameObject InstantiateSync(Transform transform=null)
        {
            return Object.Instantiate(assetObj as GameObject, transform);
        }
        internal void RealRelease()
        {
            Resources.UnloadAsset(assetObj);
        }
#endif
    }
    public class AssetAllRef {
        public int refCount;
        float lastuseTime;
        public bool CanRelease(float curTime) => refCount == 0 && curTime - lastuseTime > AssetLoadUtil.RELEASE_TIME;
        public void Release()
        {
            refCount -= 1;
            lastuseTime = Time.realtimeSinceStartup;
        }
        public AssetAllRef Retain()
        {
            refCount += 1;
            return this;
        }
#if USE_YOO
        public AllAssetsHandle assetObjs;
        public bool IsDone => assetObjs.IsDone;
        public bool IsValid => IsDone && assetObjs.IsValid;
        public AssetAllRef(AllAssetsHandle allAssetsHandle)
        {
            this.assetObjs = allAssetsHandle;
        }
        public T[] GetAssets<T>() where T : Object
        {
            var len = assetObjs.AllAssetObjects.Count;
            var ass = new T[len];
            for (int i = 0; i < len; ++i) ass[i] = assetObjs.AllAssetObjects[i] as T;
            return ass;
        }
        public UniTask ToUniTask()
        {
            return assetObjs.ToUniTask();
        }
        internal void RealRelease()
        {
            assetObjs.Release();
        }
#else
        public Object[] assetObjs;
        public bool IsDone=> true;
        public bool IsValid => assetObjs != null;
        public AssetAllRef(Object[] assetObjs)
        {
            this.assetObjs = assetObjs;
        }
        public T[] GetAssets<T>() where T: Object {
            var len = assetObjs.Length;
            var ass = new T[len];
            for (int i = 0; i < len; ++i) ass[i] = assetObjs[i] as T;
            return ass;
        }
        public UniTask ToUniTask()
        {
            return UniTask.CompletedTask;
        }
        internal void RealRelease()
        {
            var len = assetObjs.Length;
            for (int i = 0; i < len; ++i) Resources.UnloadAsset(assetObjs[i]);
        }
#endif
    }
    public class AssetLoadUtil
    {
        public const float RELEASE_TIME = 60;
        public static readonly Dictionary<string, AssetRef> AssetCacheDic = new Dictionary<string, AssetRef>();
        public static readonly Dictionary<string, AssetAllRef> AssetAllCacheDic = new Dictionary<string, AssetAllRef>();
        public static AssetRef LoadAssetAsync<T>(string path) where T : Object
        {
            return LoadAssetAsync(path,typeof(T));
        }
        public static AssetRef LoadAssetAsync(string path,Type type)
        {
#if USE_YOO
            if (AssetCacheDic.ContainsKey(path) && AssetCacheDic[path].IsValid) return AssetCacheDic[path].Retain();
            var res = new AssetRef(YooAssets.LoadAssetAsync(path, type));
#else
            path = path.Substring(0, path.IndexOf('.'));
            if (AssetCacheDic.ContainsKey(path)) return AssetCacheDic[path].Retain();
            var res = new AssetRef(Resources.Load(path, type));
#endif
            AssetCacheDic[path] = res;
            return res.Retain();
        }
        public static AssetRef LoadAssetSync(string path, Type type)
        {
#if USE_YOO
            if (AssetCacheDic.ContainsKey(path) && AssetCacheDic[path].IsValid) return AssetCacheDic[path].Retain();
            var res = new AssetRef(YooAssets.LoadAssetSync(path, type));
#else
            path = path.Substring(0, path.IndexOf('.'));
            if (AssetCacheDic.ContainsKey(path)) return AssetCacheDic[path].Retain();
            var res = new AssetRef(Resources.Load(path, type));
#endif
            AssetCacheDic[path] = res;
            return res.Retain();
        }
        public static AssetAllRef LoadAllAssetsAsync<T>(string path) where T : Object
        {
#if USE_YOO
            if (AssetAllCacheDic.ContainsKey(path) && AssetCacheDic[path].IsValid) return AssetAllCacheDic[path].Retain();
            var res = new AssetAllRef(YooAssets.LoadAllAssetsAsync<T>(path));
#else
            path = Path.GetDirectoryName(path);
            if (AssetAllCacheDic.ContainsKey(path)) return AssetAllCacheDic[path].Retain();
            var res = new AssetAllRef(Resources.LoadAll<T>(path));
#endif
            AssetAllCacheDic[path] = res;
            return res.Retain();
        }
        public static async UniTask LoadScene(string path) 
        {
            var sceneName = Path.GetFileNameWithoutExtension(path);
#if USE_YOO
            await YooAssets.LoadSceneAsync(path);
#else
            SceneManager.LoadScene(sceneName);
#endif
            await UniTask.Yield();//等待一帧，先让对应Instance Awake完毕
        }
        public static void UnloadUnusedAsset() {
            UnloadUnusedAssetInner().Forget();
        }
        public static async UniTask UnloadUnusedAssetInner() {
            // 实现自己的卸载逻辑 实现LRU策略，避免频繁卸载加载
            var keys = new HashSet<string>();
            var curTime = Time.realtimeSinceStartup;
            foreach (var kv in AssetCacheDic) {
                if (kv.Value.CanRelease(curTime)) {
                    keys.Add(kv.Key);
                }
            }
            foreach (var key in keys) {
                var val = AssetCacheDic[key];
                AssetCacheDic.Remove(key);
                val.RealRelease();
            }

#if UNITY_EDITOR
            if(keys.Count>0) Debug.Log($"正式卸载单资源[{keys.Count}]个:{string.Join('\n',keys)}");
#endif

            keys.Clear();
            foreach (var kv in AssetAllCacheDic)
            {
                if (kv.Value.CanRelease(curTime))
                {
                    keys.Add(kv.Key);
                }
            }
            foreach (var key in keys)
            {
                var val = AssetAllCacheDic[key];
                AssetAllCacheDic.Remove(key);
                val.RealRelease();
            }

#if UNITY_EDITOR
            if (keys.Count > 0) Debug.Log($"正式卸载组资源[{keys.Count}]组:{string.Join('\n', keys)}");
#endif

#if USE_YOO
            await YooAsset.YooAssets.GetPackage(Aot.AotConfig.PACKAGE_NAME).UnloadUnusedAssetsAsync();
#endif
            await Resources.UnloadUnusedAssets();
            //以非附加方式加载场景。这样会销毁当前场景中的所有对象并自动调用 Resources.UnloadUnusedAssets。
        }
    }
}
