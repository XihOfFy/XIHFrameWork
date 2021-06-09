using System;
using System.Collections.Generic;
using UnityEngine;
using XIHBasic;

namespace XIHHotFix {
    public abstract class AbsComponent
    {
        public MonoDotBase MonoDot { get; }
        protected AbsComponent(MonoDotBase dot) {
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
}
