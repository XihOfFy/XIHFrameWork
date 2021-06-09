using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Intepreter;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace XIHBasic
{
    /// <summary>
    /// Update等相关调用放入全局中，避免反射放在Update中
    /// </summary>
    public class MonoDotBase : MonoBehaviour
    {
        public string hotFixTypeName;
        [SerializeField]
        private List<GameObject> gameObjArr;
        public Dictionary<string, GameObject> GameObjsDic { get; private set; } = new Dictionary<string, GameObject>();

#if UNITY_EDITOR
        private ILTypeInstance instance;
#endif
        public Action onEnable;
        public Action onDisable;
        public Action onDestory;
        private void Awake()
        {
            var domain = HotFixBridge.Appdomain;
            if (domain == null || !domain.LoadedTypes.ContainsKey(hotFixTypeName))
            {
                Debug.LogError($"未在热更DLL中找到{hotFixTypeName}类");
                Destroy(this);
                return;
            }
            foreach (var gb in gameObjArr)
            {
                if (GameObjsDic.ContainsKey(name))
                {
                    Debug.LogError($"请勿将相同名字的物体放入链表中，{gb.name}");
                    continue;
                }
                GameObjsDic.Add(gb.name, gb);
            }
#if UNITY_EDITOR
            instance =
#endif
        domain.Instantiate(hotFixTypeName, new object[] { this });//装箱拆箱操作，有GC
        }
        private void OnEnable()
        {
            onEnable?.Invoke();
        }
        private void OnDisable()
        {
            onDisable?.Invoke();
        }
        private void OnDestroy()
        {
            onDestory?.Invoke();
        }
    }
}
