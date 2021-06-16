using System;
using System.Collections.Generic;
using UnityEngine;
using XIHBasic;

namespace XIHHotFix {
    public abstract class AbsComponent<Mono> where Mono : MonoManual
    {
        public Mono MonoDot { get; }
        protected AbsComponent(Mono dot)
        {
            MonoDot = dot;
            dot.onEnable = OnEnable;
            dot.onDisable = OnDisable;
            dot.onDestory = OnDestory;
            Awake();
        }
        protected abstract void Awake();
        protected abstract void OnEnable();
        protected abstract void OnDisable();
        protected abstract void OnDestory();
    }
    public abstract class AbsSingletonComponent<T> : AbsComponent<MonoManual> where T: AbsSingletonComponent<T>
    {
        protected AbsSingletonComponent(MonoManual dot) : base(dot) {}
        private static T instance;
        public static T Instance {
            get {
                if (instance == null)
                {
                    var dot = new GameObject().AddComponent<MonoManual>();
                    string typeName = typeof(T).FullName;
                    dot.Inject(typeName);
                    dot.name = typeName;
                }
                return instance;
            }
        }
        protected override void Awake()
        {
            if (instance != null)
            {
                GameObject.Destroy(MonoDot.gameObject);
                return;
            }
            instance = this as T;
            GameObject.DontDestroyOnLoad(MonoDot.gameObject);
        }
    }
}
