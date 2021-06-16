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
    /// Awake时调用<see cref="MonoManual.Inject(string)"> ，实现脚本自动注入
    /// Update等相关调用放入全局中，避免反射放在Update中
    /// </summary>
    public class MonoDotBase : MonoManual
    {
        public string hotFixTypeName;
        [SerializeField]
        private List<GameObject> gameObjArr;
        public Dictionary<string, GameObject> GameObjsDic { get; private set; } = new Dictionary<string, GameObject>();
        private void Awake()
        {
            foreach (var gb in gameObjArr)
            {
                if (GameObjsDic.ContainsKey(name))
                {
                    Debug.LogError($"请勿将相同名字的物体放入链表中，{gb.name}");
                    continue;
                }
                GameObjsDic.Add(gb.name, gb);
            }
            Inject(hotFixTypeName);
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
