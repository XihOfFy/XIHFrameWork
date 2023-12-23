namespace FairyGUI
{
	public static class GObjectExtensions
	{
		/// <summary>
		/// 设置可见性
		/// </summary>
		public static void SetVisible(this GObject obj, bool value)
		{
			if (obj != null)
				obj.visible = value;
		}

		/// <summary>
		/// 设置灰态+点击穿透
		/// </summary>
		public static void SetEnabled(this GObject obj, bool value)
		{
			if (obj != null)
				obj.enabled = value;
		}

		/// <summary>
		/// 设置灰态
		/// </summary>
		public static void SetGrayed(this GObject obj, bool value)
		{
			if (obj != null)
				obj.grayed = value;
		}
	}
}
