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
        [SerializeField]
        private string hotFixTypeFullName;
        public string HotFixTypeName => hotFixTypeFullName;
        private ILTypeInstance instance = null;
        public ILTypeInstance ILTypeInstance => instance;
        public Action onEnable;
        public Action onDisable;
        public Action onDestory;
        public ILTypeInstance Inject(string hotFixTypeName)
        {
            if (string.IsNullOrEmpty(hotFixTypeName)) {
                Debug.LogWarning($"代码动态添加可忽略此消息，否则检查物体组件{GetType()}将{nameof(hotFixTypeName)}参数写上对应热更类");
                return null;
            }
            if (instance!=null) {
                Debug.LogWarning("已经Inject过，无需重复注入");
                return instance;
            }
            var domain = HotFixBridge.Appdomain;
            if (domain == null || !domain.LoadedTypes.ContainsKey(hotFixTypeName))
            {
                Debug.LogError($"未在热更DLL中找到{hotFixTypeName}类");
                Destroy(this);
                return null;
            }
            this.hotFixTypeFullName = hotFixTypeName;
            instance = domain.Instantiate(hotFixTypeName, new object[] { this });//装箱拆箱操作，有GC
            return instance;
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
