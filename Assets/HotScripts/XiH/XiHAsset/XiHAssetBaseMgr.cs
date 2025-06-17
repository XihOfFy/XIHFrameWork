using Aot.XiHUtil;
using Cysharp.Threading.Tasks;
using FairyGUI;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace XiHAsset
{
    public abstract partial class XiHAssetBaseMgr : MonoBehaviour
    {
        public const float ASSET_CHECK_TIME = AssetLoadUtil.RELEASE_TIME / 2;
        public static float AssetLifeTime;
        CancellationTokenSource cancellationTokenSource;
        public CancellationToken cancellationToken;
        public static XiHAssetBaseMgr BaseInstance { get; private set; }
        Dictionary<string, AssetRef> assHandles;
#if UNITY_WX
        public WeChatWASM.WXFeedbackButton feedbackBtn;
#endif 
        private void Awake()
        {
            if (BaseInstance != null)
            {
                Debug.LogError($"存在多个 XiHAssetBaseMgr {GetType()}");
                DestroyImmediate(this.gameObject);
                return;
            }
            BaseInstance = this;
            assHandles = new Dictionary<string, AssetRef>();
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
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
            assHandles.Clear();
            cancellationTokenSource.Cancel();
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
            await handle.ToUniTask(cancellationToken:cancellationToken);
            return handle;
        }
        private void LateUpdate()
        {
            AssetLifeTime += Time.deltaTime;
            if (AssetLifeTime > ASSET_CHECK_TIME) {
                AssetLifeTime = 0;
                AssetLoadUtil.UnloadUnusedAsset();//先执行本地卸载，然后再执行实际卸载
            }
        }
        public async UniTask SetEffect4GLoader3D(GLoader3D loader, string effPath, bool destoryOld = false)
        {
            if (loader.wrapTarget == null)
            {
                var effHandle = await GetHandle<GameObject>(effPath);
                if (loader.isDisposed) return;
                loader.SetWrapTarget(effHandle.InstantiateSync(), false, 1, 1);
            }
            else
            {
                if (destoryOld)
                {
                    GameObject.Destroy(loader.wrapTarget);
                    var effHandle = await GetHandle<GameObject>(effPath);
                    if (loader.isDisposed) return;
                    loader.SetWrapTarget(effHandle.InstantiateSync(), false, 1, 1);
                }
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
