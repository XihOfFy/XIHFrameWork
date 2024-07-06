using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using FairyGUI;
using UnityEngine;
using XiHUtil;

namespace XiHUI
{
	/// <summary>
	/// 自动绑定: 目标对象的成员变量<->对应组件内同名控件
	/// </summary>
	public static class UIControlBinding
	{
		private static Type _controllType = typeof(Controller);
		private static Type _transitionType = typeof(Transition);
		private static Type _componentType = typeof(GComponent);
		private static Type _uiobjectType = typeof(IUIObjectWrap);

		private static BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

		//private static string LeiSheTag = "leishe";

		public static void BindFields(object obj, GObject root)
		{
			if (root == null || obj == null)
				return;

			var rootComp = root as GComponent;
			if (rootComp == null)
				return;

			var objType = obj.GetType();
			if (!objType.IsClass)
				return;

			var parentUI = obj as UIDialog;
			var parentWrap = obj as IUIObjectWrap;

			// 通过反射获取所有成员变量, 只保留可绑定控件类型
			var fields = new List<FieldInfo>();
			ReflectUtil.GetFields(objType, flags, true, ref fields, CanBind);

			// 广度遍历获取所有控件
			var controls = GetAllControls(rootComp);

			for (int i = 0; i < fields.Count; i++)
			{
				var field = fields[i];
				if (field == null)
					continue;

				// 匹配名称时，删除变量的_前缀
				var name = field.Name.StartsWith("_") ? field.Name.Substring(1) : field.Name;
				var type = field.FieldType;

				// 优先绑定父级同名控制器
				if (type == _controllType)
				{
					var c = rootComp.GetController(name);
					if (c != null)
					{
						field.SetValue(obj, c);
						continue;
					}
				}

				// 优先绑定父级同名动效
				if (type == _transitionType)
				{
					var t = rootComp.GetTransition(name);
					if (t != null)
					{
						field.SetValue(obj, t);
						continue;
					}
				}

				// 存在多个同名控件时，优先绑定第一个同名控件。代码设置组件样式无效时，请检查UI组件命名
				foreach (var control in controls)
				{
					if (!name.Equals(control.name, StringComparison.OrdinalIgnoreCase))
						continue;

					if (_uiobjectType.IsAssignableFrom(field.FieldType))
					{
						var wrap = Activator.CreateInstance(field.FieldType) as IUIObjectWrap;
						if (!wrap.SetPayload(control))
							Debug.LogError($"{objType.Name}绑定[{field.FieldType.Name}  {field.Name}]时，其同名UI控件{control.name}的类型<{control.GetType().Name}>不是所指定类型<{wrap.ContentType().Name}>\n可能导致逻辑错误，请检查代码或UI工程中界面使用的控件类型");
						field.SetValue(obj, wrap);
						parentUI?.AddChild(wrap);
						parentWrap?.AddChild(wrap);
						break;
					}
					else if (field.FieldType.IsAssignableFrom(control.GetType()))
					{
						field.SetValue(obj, control);
						break;
					}
				}
			}

		}

		private static bool CanBind(FieldInfo field)
		{
			if (field == null)
				return false;

			if (field.FieldType.IsGenericType)
				return _uiobjectType.IsAssignableFrom(field.FieldType);

			// 控制器和动画
			if (field.FieldType == _controllType || field.FieldType == _transitionType)
				return true;

			// UI控件
			if (typeof(GObject).IsAssignableFrom(field.FieldType))
			{
				// 排除Window
				return !typeof(Window).IsAssignableFrom(field.FieldType);
			}

			// UI控件包装器
			if (_uiobjectType.IsAssignableFrom(field.FieldType))
				return true;

			// 其他类型直接忽略
			return false;
		}


		// 广度遍历获取所有控件
		private static IReadOnlyList<GObject> GetAllControls(GComponent root)
		{
			var controls = new List<GObject>();
			GetChildControls(root, controls);

			for (int i = 0; i < controls.Count; i++)
			{
				if (_componentType.IsAssignableFrom(controls[i].GetType()))
					GetChildControls((GComponent)controls[i], controls);
			}

			return controls;
		}

		private static void GetChildControls(GComponent parent, IList<GObject> controls)
		{
			int count = parent.numChildren;
			for (int i = 0; i < count; i++)
				controls.Add(parent.GetChildAt(i));
		}

	}
}
