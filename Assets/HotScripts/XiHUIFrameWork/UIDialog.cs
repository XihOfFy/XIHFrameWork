using FairyGUI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace XiHUI
{
    public enum Mode
    {
        None = 0,

        Stack,      // 全屏栈窗口
        Popup,      // 弹出式子窗口
        Modal,      // 模态弹出框
        TopMost,    // 顶层窗口(如Loading)

        Max
    }

    public enum State
    {
        None = 0,

        Loading,
        Open,
        Hide,
        Close,
    }

    /// <summary>
    /// 基础UI窗口
    /// </summary>
    public class UIDialog
    {
        public State State { get; private set; }

        public DialogOpenParams OpenParams { get; private set; }

        public string dialogName { get; protected set; }

        public event Action<UIDialog> OnDispose;

        public GComponent Content { get; protected set; }
        public bool IsFullScreen { get; protected set; }
        public bool IsBlurBG { get; protected set; }

        public List<IUIObjectWrap> _children = new List<IUIObjectWrap>();

        public List<UIDialog> BaseDialogs { get; private set; }

        public UIDialog()
        {
            State = State.Loading;
            BaseDialogs = new List<UIDialog>();
        }

        // 转跳界面后传递参数，各个界面根据需求重写解析逻辑
        public virtual void InitParam(params string[] param)
        {
        }

        internal void SetOpenParams(DialogOpenParams openParams)
        {
            OpenParams = openParams;
            dialogName = OpenParams.DialogName;
        }

        internal void SetBase(UIDialog dialog)
        {
            if (dialog == null)
                return;

            if (!BaseDialogs.Contains(dialog))
                BaseDialogs.Add(dialog);
        }

        internal void AddChild(IUIObjectWrap wrap)
        {
            if (!_children.Contains(wrap))
                _children.Add(wrap);
        }

        internal void Open(GComponent obj, bool isFull, bool isBlur)
        {
            Content = obj;
            IsFullScreen = isFull;
            IsBlurBG = isBlur;
            if (Content != null)
            {
                Content.name = dialogName;
                Content.fairyBatching = false;
#if UNITY_EDITOR
                Debug.LogWarning($"关闭 fairyBatching 避免动效UI展示错误");
#endif
            }

            InitComponent();
            State = State.Open;
        }

        internal void Open()
        {
            State = State.Open;
            Content?.SetVisible(true);
            OnOpen();
        }

        internal void Close()
        {
            UIDialogManager.Instance.Close(this);
        }

        internal void Dispose()
        {
            foreach (var c in _children)
                c?.Dispose();
            if (State != State.Hide)//新增，避免生命周期直接销毁UI没有hide
                Hide();

            State = State.Close;
            OnClose();
            var act = OnDispose;
            OnDispose = null;
            Content?.Dispose();
            BaseDialogs = null;
            act?.Invoke(this);
        }

        internal void Hide()
        {
            if (State == State.Hide)
                return;
            State = State.Hide;
            Content?.SetVisible(false);
            OnHide();//新增，避免生命周期没有Onhide

        }

        internal void Update()
        {
            foreach (var c in _children)
                c?.Update();

            OnUpdate();
        }

        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
        protected virtual void OnHide() { }
        protected virtual void OnUpdate() { }
        protected virtual void InitComponent() { }
    }
}
