using System;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 列表控件支持库
	/// 让非虚拟列表也使用itemRenderer来刷新数据
	/// </summary>
	public static class GListExtensions
	{
		/// <summary>
		/// 注册itemRenderer, 强制要求控件绑定为特定子类型
		/// </summary>
		public static void SetItemRenderer<T>(this GList list, Action<int, T> itemRenderer) where T:GObject
		{
			if (list == null)
				return;

			list.itemRenderer = (index, item) => {
				var listItem = item as T;
				if (listItem == null)
				{
					Debug.LogWarning($"GListExtensions.ItemRenderer({item.name}) invalid control {typeof(T).Name}");
					return;
				}

				itemRenderer.Invoke(index, listItem);
			};
		}

		/// <summary>
		/// 注册点击回调, 将点击条目的data转化为回调参数ID
		/// </summary>
		public static void SetItemClickIdHandler<TItemId>(this GList list, Action<TItemId> clickIdHandler)
		{
			if (list == null)
				return;

			list.onClickItem.Add((c) => {
				var id = default(TItemId);

				var listItem = c.data as GObject;
				if (listItem != null && listItem.data != null)
					id = (TItemId)listItem.data;
				
				clickIdHandler.Invoke(id);
			});
		}

		/// <summary>
		/// 设置条目数量
		/// </summary>
		public static void SetItemCount(this GList list, int count)
		{
			if (list == null)
				return;

			list.numItems = count;
		}

		/// <summary>
		/// 刷新所有条目
		/// </summary>
		/// <param name="list"></param>
		public static void UpdateAllItems(this GList list)
		{
			if (list == null)
				return;

			if (list.isVirtual)
			{
				list.RefreshVirtualList();
			}
			else
			{
				var itemRenderer = list.itemRenderer;
				if (itemRenderer == null)
					return;

				for (int i = 0; i < list.numItems; i++)
					itemRenderer.Invoke(i, list.GetChildAt(i));
			}
		}

		/// <summary>
		/// 刷新某个下标特定条目(如果是虚拟列表则全刷新)
		/// </summary>
		public static void UpdateItem(this GList list, int index)
		{
			if (list.isVirtual)
			{
				// 虚拟列表只支持全刷新
				list.RefreshVirtualList();
			}
			else
			{
				if (index < 0 || index >= list.numChildren)
				{
					Debug.LogWarning($"GListExtensions.UpdateItem({index}) out of {list.numChildren}");
					return;
				}

				var itemRenderer = list.itemRenderer;
				if (itemRenderer == null)
					return;

				itemRenderer.Invoke(index, list.GetChildAt(index));
			}
		}
	}
}
