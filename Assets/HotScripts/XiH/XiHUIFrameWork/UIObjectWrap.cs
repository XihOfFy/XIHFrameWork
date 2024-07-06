using FairyGUI;
using System;
using System.Collections.Generic;

namespace XiHUI
{
	public interface IUIObjectWrap
	{
		bool SetPayload(GObject gObject);
		Type ContentType();

		void Update();
		void Dispose();
		void AddChild(IUIObjectWrap wrap);
	}

	/// <summary>
	/// 原生GObject对象包装器，可用于反射绑定组件
	/// 可作为 UIComponent，UIButton 等的基类
	/// </summary>
	public class UIObjectWrap<T> : IUIObjectWrap where T : GObject
	{
		public T content { get; protected set; }

		public Type ContentType() => typeof(T);

		public static implicit operator T(UIObjectWrap<T> v) => v.content;

		public UIObjectWrap() { }

		public UIObjectWrap(T content) => SetPayload(content);

		public List<IUIObjectWrap> _children = new List<IUIObjectWrap>();

		public bool SetPayload(GObject content)
		{
			this.content = content as T;
			UIControlBinding.BindFields(this, content);
			InitComponent();

			return this.content != null;
		}

		public void AddChild(IUIObjectWrap wrap)
		{
			if (!_children.Contains(wrap))
				_children.Add(wrap);
		}

		/// <summary>
		/// 组件初始化时调用一次，可在此处进行初始化，事件注册等操作
		/// </summary>
		protected virtual void InitComponent() { }

		public virtual void Update()
		{
			foreach (var c in _children)
				c?.Update();
		}

		public virtual void Dispose()
		{
			foreach (var c in _children)
				c?.Dispose();

			content?.RemoveEventListeners();
		}

		public bool visible => content != null ? content.visible : false;
		public void SetVisible(bool v) => content?.SetVisible(v);
	}
}
