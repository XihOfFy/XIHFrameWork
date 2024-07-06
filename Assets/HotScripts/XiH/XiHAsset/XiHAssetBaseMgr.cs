using Cysharp.Threading.Tasks;
using FairyGUI;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace XiHAsset
{
    /// <summary>
    /// 场景资源管理器，每个场景只能存在一个，管理Yooasset AB资源加载和卸载，子类的Awake和OnDestroy使用SetInstance和DestoryInstance替换
    /// </summary>
    public  abstract partial class XiHAssetBaseMgr : MonoBehaviour
    {
        public static XiHAssetBaseMgr BaseInstance { get; private set; }
        Dictionary<string, AssetHandle> assHandles;
        bool destoryed;
        private void Awake()
        {
            destoryed = false;
            if (BaseInstance != null)
            {
                Debug.LogError($"存在多个 AssetMgr {GetType()}");
                DestroyImmediate(this.gameObject);
                return;
            }
            BaseInstance = this;
            assHandles = new Dictionary<string, AssetHandle>();
            SetInstance();
        }
        protected abstract void SetInstance();
        protected abstract void DestoryInstance();
        private void OnDestroy()
        {
            destoryed = true;
            if (BaseInstance != this) return;
            BaseInstance = null;
            DestoryInstance();
            foreach (var handle in assHandles)
            {
                handle.Value.Release();
            }
            assHandles.Clear();
        }
        public async UniTask<AssetHandle> GetHandle<T>(string path) where T : Object
        {
            if (assHandles.ContainsKey(path))
            {
                var tmp = assHandles[path];
                await UniTask.WaitUntil(() => tmp.IsDone);

                await UniTask.WaitUntil(() => tmp.IsDone || !tmp.IsValid);
                if (!tmp.IsValid)
                {
                    assHandles.Remove(path);
                    Debug.LogWarning($"此特效{path}已经释放,无法使用");
                    if (destoryed) throw new MonoDestoryedException();//场景切换了
                    return await GetHandle<T>(path);
                }
                if (destoryed) {
                    tmp.Release();
                    throw new MonoDestoryedException();//场景切换了
                }
                return tmp;
            }
            var handle = YooAssets.LoadAssetAsync<T>(path);
            assHandles.Add(path, handle);
            await handle.ToUniTask();
            if (destoryed) { 
                handle.Release();
                throw new MonoDestoryedException();//场景切换了
            }
            return handle;
        }
    }
    /// <summary>
    /// 场景管理器，为了单例支持泛型，管理Yooasset AB资源加载和卸载，子类的Awake和OnDestroy使用SetInstance和DestoryInstance替换
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
