using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YooAsset;
using Aot.XiHUtil;

namespace XiHAsset
{
    public class AssetPoolMgr
    {
        const int MIN_CNT = 4;
        Transform poolRootTrs;
        protected static AssetPoolMgr instance;
        public static AssetPoolMgr Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AssetPoolMgr();
                    instance.poolRootTrs = new GameObject(nameof(AssetPoolMgr)).transform;
                    GameObject.DontDestroyOnLoad(instance.poolRootTrs);
                }
                return instance;
            }
        }
        Dictionary<string, GameObject> prefabDic;//持久化存储的handle，不进行释放
        Dictionary<string, int> preGenCapacityDic;//容量大小
        Dictionary<string, Queue<GameObject>> objPool;
        Dictionary<string, HashSet<GameObject>> objOutPool;
        public async UniTask InitPool(string poolPath)
        {
            //SceneManager.sceneUnloaded += Recycle;

            GameObject[] objs = Array.Empty<GameObject>();
#if USE_YOO
            if (YooAssets.CheckLocationValid(poolPath))
            {
                var allHandle = AssetLoadUtil.LoadAllAssetsAsync<GameObject>(poolPath);
                await allHandle.ToUniTask();//不进行release，所以不存储
                objs = allHandle.GetAssets<GameObject>();
            }
            else
            {
                Debug.LogError("若使用对象池，记得修改路径和添加对于预制体");
            }
#endif
            prefabDic = new Dictionary<string, GameObject>(objs.Length);
            preGenCapacityDic = new Dictionary<string, int>(objs.Length);
            foreach (var obj in objs)
            {
                var name = obj.name;
                var idx = name.LastIndexOf('_');
                var key = name.Substring(0, idx);
                int.TryParse(name.Substring(idx + 1, name.Length - idx - 1), out var val);
                prefabDic[key] = obj;
                preGenCapacityDic[key] = val;
            }

            objPool = new Dictionary<string, Queue<GameObject>>(objs.Length);
            objOutPool = new Dictionary<string, HashSet<GameObject>>(objs.Length);

            foreach (var kv in preGenCapacityDic)
            {
                var obj = prefabDic[kv.Key];
                var loopCnt = kv.Value;
                while (loopCnt-- > 0)
                {
                    Return(kv.Key, GameObject.Instantiate(obj));
                }
            }
        }
        public void Recycle()
        {
            var keys = objOutPool.Keys.ToList();
            foreach (var key in keys)
            {
                var val = objOutPool[key];
                foreach (var v in val)
                {
                    if (v)
                    {
                        Return(key, v);
                    }
                }
                val.Clear();
            }
        }
        public GameObject Spwan(string path)
        {
            if (objPool.TryGetValue(path, out var que))
            {
                if (que.Count > 0)
                {
                    var obj = que.Dequeue();
                    InitSpwanObj(path, obj);
                    return obj;
                }
            }
            if (prefabDic.TryGetValue(path, out var handle))
            {
                var insObj = GameObject.Instantiate(handle);
                GameObject.DontDestroyOnLoad(insObj);
                InitSpwanObj(path, insObj);
                return insObj;
            }
            else
            {
                Debug.LogError("缺少初始化InitPool添加该对象进行池化:" + path);
            }
            return null;
        }
        void InitSpwanObj(string path, GameObject insObj)
        {
            insObj.transform.SetParent(null);
            insObj.SetActive(true);
            ReturnOutPool(path, insObj);
        }
        public void Return(GameObject obj, string path = null)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (objOutPool.TryGetValue(path, out var set))
                {
                    set.Remove(obj);
                    Return(path, obj);
                    return;
                }
            }
            foreach (var kv in objOutPool)
            {
                if (kv.Value.Contains(obj))
                {
                    kv.Value.Remove(obj);
                    Return(kv.Key, obj);
                    return;
                }
            }
        }
        void Return(string path, GameObject obj)
        {
            //Debug.Log("Return "+ path,obj);
            if (!objPool.TryGetValue(path, out var que))
            {
                que = new Queue<GameObject>(preGenCapacityDic[path]);
                objPool[path] = que;
            }
            obj.SetActive(false);
            if (obj.TryGetComponent<IRecycled>(out var poolable))
            {
                poolable.Recycled();
            }
            obj.transform.SetParent(poolRootTrs);
            if (que.Contains(obj))
            {
                Debug.LogError("之前已经回收到池子"+ path, obj);
                return;
            }
            que.Enqueue(obj);
        }
        void ReturnOutPool(string path, GameObject obj)
        {
            if (!objOutPool.TryGetValue(path, out var set))
            {
                set = new HashSet<GameObject>(preGenCapacityDic[path]);
                objOutPool[path] = set;
            }
            if (set.Contains(obj))
            {
                Debug.LogError("之前已经被Spwan，无法继续使用" + path, obj);
                return;
            }
            set.Add(obj);
        }
    }
    public interface IRecycled {
        public const string POOL_OBJ_PATH = "TestObj";//不需要_X后缀
        void Recycled();
    }
}
