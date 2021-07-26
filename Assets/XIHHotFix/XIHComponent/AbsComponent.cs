using System;
using System.Collections.Generic;
using System.Linq;
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
    public abstract class AbsSingletonComponent<T, Mono> : AbsComponent<Mono> where T: AbsSingletonComponent<T, Mono> where Mono: MonoManual
    {
        protected AbsSingletonComponent(Mono dot) : base(dot) {}
        private static T instance;
        public static T Instance {
            get {
                if (instance == null)
                {
                    var gameObject = new GameObject(typeof(T).FullName);
                    instance = gameObject.AddComponent<Mono, T>();
                    GameObject.DontDestroyOnLoad(gameObject);
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
        }
    }
    public static class ComponentExt {
        public static HotType AddComponent<ComMono,HotType>(this MonoManual mono) where ComMono : MonoManual where HotType : AbsComponent<ComMono>
        {
           return AddComponent<ComMono,HotType>(mono.gameObject);
        }
        public static HotType AddComponent<ComMono,HotType>(this GameObject gameObject)where ComMono : MonoManual where HotType : AbsComponent<ComMono>
        {
            var dot = gameObject.AddComponent<ComMono>();
            string typeName = typeof(HotType).FullName;
            return dot.Inject(typeName) as HotType;//将调用Awake()方法
        }
        public static HotType GetComponent<ComMono,HotType>(this MonoManual mono) where ComMono : MonoManual where HotType : AbsComponent<ComMono>
        {
            var res = mono.GetComponents<ComMono, HotType>();
            var rs = res.FirstOrDefault();
            if (rs == null) return null;
            return rs;
        }
        public static IEnumerable<HotType> GetComponents<ComMono, HotType>(this MonoManual mono) where ComMono : MonoManual where HotType : AbsComponent<ComMono>
        {
            string filter = typeof(HotType).FullName;
            var cps = mono.GetComponents<ComMono>();
            return cps.Where(cp => cp.HotFixTypeName == filter).Select(cp=>cp.ILTypeInstance as HotType);
        }
    }
}
