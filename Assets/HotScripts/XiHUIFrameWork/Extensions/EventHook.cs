
namespace FairyGUI
{
	public static class EventHook
	{
		public delegate void HookCallback(GObject comp);

		public static event HookCallback onClick;
		public static event HookCallback onTouchBegin;

		public static void HookBubbleEvent(EventDispatcher dispatcher, string strType, object data)
		{
			if (dispatcher == null || string.IsNullOrEmpty(strType))
				return;

			if (strType == "onClick")
			{
				if (dispatcher is DisplayObject dispObj)
					onClick?.Invoke(dispObj.gOwner);
			}
			else if (strType == "onTouchBegin")
			{
				if (dispatcher is DisplayObject dispObj)
					onTouchBegin?.Invoke(dispObj.gOwner);
			}
		}
	}
}
