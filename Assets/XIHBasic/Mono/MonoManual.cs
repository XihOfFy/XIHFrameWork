using ILRuntime.Runtime.Intepreter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XIHBasic
{
    /// <summary>
    /// 热更脚本手动调用<see cref="MonoManual.Inject(string)"> ，实现脚本手动注入
    /// </summary>
    public class MonoManual : MonoBehaviour
    {
        protected ILTypeInstance instance;
        public Action onEnable;
        public Action onDisable;
        public Action onDestory;
        public void Inject(string hotFixTypeName)
        {
            if (instance!=null) {
                Debug.LogWarning("已经Inject过，无需重复注入");
                return;
            }
            var domain = HotFixBridge.Appdomain;
            if (domain == null || string.IsNullOrEmpty(hotFixTypeName) || !domain.LoadedTypes.ContainsKey(hotFixTypeName))
            {
                Debug.LogError($"未在热更DLL中找到{hotFixTypeName}类");
                Destroy(this);
                return;
            }
            instance = domain.Instantiate(hotFixTypeName, new object[] { this });//装箱拆箱操作，有GC
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
