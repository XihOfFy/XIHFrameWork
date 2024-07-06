using System.Text;
using UnityEngine;

namespace FairyGUI
{
	public static class GComponentExtensions
	{
		public static bool ShowControlPath = false;

		private static StringBuilder _strBuilder = new StringBuilder();

		/// Get from full path: GRoot/***Layer/***Dialog/controlPath
		public static string GetControlPath(this GObject comp)
		{
#if UNITY_EDITOR
			UnityEngine.Profiling.Profiler.BeginSample("GComponentExtensions.GetControlPath");
			try
#endif
			{

				if (comp == null)
					return null;

				_strBuilder.Clear();
				_strBuilder.Append(comp.name);

				var parent = comp.parent;
				while (parent != null)
				{
					var name = parent.name;
					if (string.IsNullOrEmpty(name) || name == "GRoot")
						break;

					_strBuilder.Insert(0, '/').Insert(0, name);
					parent = parent.parent;
				}

				var result = _strBuilder.ToString();

				//// filter ***Dialog/
				//int p = result.IndexOf('/');
				//if (p >= 0)
				//	result = result.Substring(p + 1);

				return result;

			}
#if UNITY_EDITOR
			finally
			{

				UnityEngine.Profiling.Profiler.EndSample();
			}
#endif
		}

		public static Vector3 GetRootPos(this GObject comp, bool center = true)
		{
			if (comp == null)
				return Vector3.zero;
			Vector3 offset = Vector3.zero;
			if (center)
			{
				if (comp.pivotAsAnchor)
					offset = new Vector3((.5f - comp.pivot.x) * comp.width, (.5f - comp.pivot.y) * comp.height, 0);
				else
					offset = new Vector3(.5f * comp.width, .5f * comp.height, 0);
			}
			return comp.TransformPoint(offset, GRoot.inst);
		}
	}
}
