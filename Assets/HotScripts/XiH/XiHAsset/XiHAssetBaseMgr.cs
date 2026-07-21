using Aot.XiHUtil;
using Cysharp.Threading.Tasks;
using FairyGUI;
using System.Collections.Generic;
using System.Threading;
using Tmpl;
using UnityEngine;
using UnityEngine.U2D;

namespace XiHAsset
{
    [RequireComponent(typeof(Camera))]
    public abstract partial class XiHAssetBaseMgr : MonoBehaviour
    {
        public const float ASSET_CHECK_TIME = AssetLoadUtil.RELEASE_TIME / 2;
        public static float AssetLifeTime;
        CancellationTokenSource cancellationTokenSource;
        public CancellationToken cancellationToken;
        [HideInInspector] public Camera gameCamera;
        [HideInInspector] public bool adPlaying;//广告是否在播放
        public static XiHAssetBaseMgr BaseInstance { get; private set; }
        Dictionary<string, AssetRef> assHandles;
        Dictionary<string, AssetAllRef> assAllHandles;
        private void Awake()
        {
            if (BaseInstance != null)
            {
                Debug.LogError($"存在多个 XiHAssetBaseMgr {GetType()}");
                DestroyImmediate(this.gameObject);
                return;
            }
            adPlaying = false;
            BaseInstance = this;
            assHandles = new Dictionary<string, AssetRef>(512);
            assAllHandles = new Dictionary<string, AssetAllRef>(64);
            spriteDic = new Dictionary<string, SpriteAtlas>(128);
            webTexDic = new Dictionary<string, Texture2D>(128);
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            gameCamera = GetComponent<Camera>();
            AddLifeCycle();
            SetInstance();
        }
        protected abstract void SetInstance();
        protected abstract void DestoryInstance();
        private void OnDestroy()
        {
            if (BaseInstance != this) return;
            BaseInstance = null;
            DestoryInstance();
            foreach (var handle in assHandles)
            {
                handle.Value.Release();
            }
            foreach (var handle in assAllHandles)
            {
                handle.Value.Release();
            }
            assAllHandles.Clear();
            assHandles.Clear();
            spriteDic.Clear();
            foreach (var kv in webTexDic)
            {
                if (kv.Value)
                {
                    Destroy(kv.Value);
                }
            }
            webTexDic.Clear();
            cancellationTokenSource.Cancel();
            RemoveLifeCycle();
        }
        public async UniTask<AssetRef> GetHandle<T>(string path) where T : Object
        {
            if (assHandles.ContainsKey(path))
            {
                var tmp = assHandles[path];
                //await UniTask.WaitUntil(() => tmp.IsDone, cancellationToken: cancellationToken);
                await UniTask.WaitUntil(() => tmp.IsDone || !tmp.IsValid, cancellationToken: cancellationToken);
                if (!tmp.IsValid)
                {
                    assHandles.Remove(path);
                    Debug.LogWarning($"此资源{path}已经释放,无法使用");
                    return await GetHandle<T>(path);
                }
                return tmp;
            }
            var handle = AssetLoadUtil.LoadAssetAsync<T>(path);
            assHandles.Add(path, handle);
            await handle.ToUniTask(cancellationToken: cancellationToken);
            return handle;
        }

        public async UniTask<AssetAllRef> GetAllHandles<T>(string path) where T : Object
        {
            if (assAllHandles.ContainsKey(path))
            {
                var tmp = assAllHandles[path];
                //await UniTask.WaitUntil(() => tmp.IsDone, cancellationToken: cancellationToken);
                await UniTask.WaitUntil(() => tmp.IsDone || !tmp.IsValid, cancellationToken: cancellationToken);
                if (!tmp.IsValid)
                {
                    assAllHandles.Remove(path);
                    Debug.LogWarning($"此资源{path}已经释放,无法使用");
                    return await GetAllHandles<T>(path);
                }
                return tmp;
            }
            var handle = AssetLoadUtil.LoadAllAssetsAsync<T>(path);
            assAllHandles.Add(path, handle);
            await handle.ToUniTask(cancellationToken: cancellationToken);
            return handle;
        }

        protected virtual void Update()
        {
#if USE_GM
            UpdateGM();
#endif
#if UNITY_EDITOR
            UpdateEditor();
#endif
        }
#if UNITY_EDITOR
        protected virtual void UpdateEditor()
        {
        }
#endif
        protected virtual void LateUpdate()
        {
            AssetLifeTime += Time.deltaTime;
            if (AssetLifeTime > ASSET_CHECK_TIME)
            {
                AssetLifeTime = 0;
                LocalizationExt.ClearTrsInfoCache();
                AssetLoadUtil.UnloadUnusedAsset();//先执行本地卸载，然后再执行实际卸载
            }
        }
        public async UniTask SetObj4GLoader3D(GLoader3D loader, string effPath, Vector3 localPosition = default, float existTime = -1)
        {
            var effHandle = await GetHandle<GameObject>(effPath);
            if (loader.isDisposed) return;
            var obj = effHandle.InstantiateSync();
            SetObj4GLoader3D(loader, obj, localPosition);
        }
        public void SetObj4GLoader3D(GLoader3D loader, GameObject obj, Vector3 localPosition = default, float existTime = -1)
        {
            if (loader.wrapTarget)
            {
                GameObject.Destroy(loader.wrapTarget);
            }
            loader.SetWrapTarget(obj, false, 1, 1);
            obj.transform.parent.localPosition = localPosition;
            if (existTime > 0)
            {
                GameObject.Destroy(obj, existTime);
            }
        }
    }
    public abstract class XiHAssetBaseMgr<T> : XiHAssetBaseMgr where T : XiHAssetBaseMgr<T>
    {
        public static T Instance { get; private set; }
        protected override void SetInstance()
        {
            Instance = (T)this;
        }
        protected override void DestoryInstance()
        {
            Instance = null;
        }
    }
}
