using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
namespace XiHUtil
{
	/// <summary>
	/// 反射工具箱
	/// </summary>
	public static class ReflectUtil
    {
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsList(this Type type) => typeof(IList).IsAssignableFrom(type);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsDictionary(this Type type) => typeof(IDictionary).IsAssignableFrom(type);

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Type GetGenericArg0(Type type) => GetGenericArgAt(type, 0);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Type GetGenericArg1(Type type) => GetGenericArgAt(type, 1);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Type GetGenericArg2(Type type) => GetGenericArgAt(type, 2);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Type GetGenericArgAt(Type type, int index)
		{
			if (!type.IsGenericType)
				return null;

			var arguments = type.GetGenericArguments();
			if (arguments == null || arguments.Length <= 0 || index >= arguments.Length)
				return null;

			return arguments[index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetFields(Type objType, BindingFlags bindingFlags, bool inherit, ref List<FieldInfo> result, Func<FieldInfo, bool> predicate = null)
		{
			bindingFlags |= BindingFlags.DeclaredOnly;

			if (result == null)
				result = new List<FieldInfo>();

			var fields = objType.GetFields(bindingFlags);
			for (int i = 0; i < fields.Length; i++)
			{
				var field = fields[i];
				if (field == null)
					continue;
				if (predicate != null && !predicate(field))
					continue;

				result.Add(field);
			}

			if (inherit)
			{
				objType = objType.BaseType;

				while (objType != null)
				{
					GetFields(objType, bindingFlags, false, ref result, predicate);
					objType = objType.BaseType;
				}
			}
		}

#if false
		public static FieldInfo[] GetFields<T>(string[] names) { return GetFields(typeof(T), names); }
		public static FieldInfo[] GetFields(Type objType, string[] names)
		{
			if (objType == null || names == null)
				return null;

			int fieldCount = names.Length;
			FieldInfo[] fields = new FieldInfo[fieldCount];

			for (int i = 0; i < fieldCount; ++i)
				fields[i] = objType.GetField(names[i]);

			return fields;
		}

		

		public static FieldInfo GetField<T>(string name, BindingFlags bindingFlags, bool findBaseType)
		{
			return GetField(typeof(T), name, bindingFlags, findBaseType);
		}

		public static FieldInfo GetField(Type objType, string name, BindingFlags bindingFlags, bool findBaseType)
		{
			if (objType == null || string.IsNullOrEmpty(name))
				return null;

			var field = objType.GetField(name, bindingFlags);

			if (field == null && findBaseType)
			{
				objType = objType.BaseType;

				while (objType != null)
				{
					field = objType.GetField(name, bindingFlags);
					if (field != null)
						break;

					objType = objType.BaseType;
				}
			}

			return field;
		}

		public static PropertyInfo GetProperty<T>(string name, BindingFlags bindingFlags, bool findBaseType)
		{
			return GetProperty(typeof(T), name, bindingFlags, findBaseType);
		}

		public static PropertyInfo GetProperty(Type objType, string name, BindingFlags bindingFlags, bool findBaseType)
		{
			if (objType == null || string.IsNullOrEmpty(name))
				return null;

			var property = objType.GetProperty(name, bindingFlags);

			if (property == null && findBaseType)
			{
				objType = objType.BaseType;

				while (objType != null)
				{
					property = objType.GetProperty(name, bindingFlags);
					if (property != null)
						break;

					objType = objType.BaseType;
				}
			}

			return property;
		}
#endif

		/// <summary>
		/// 获取指定名字的属性或字段
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static MemberInfo GetDataMember(Type objType, string name, bool isStatic)
		{
			if (objType == null || name == null)
				return null;

			var flags = BindingFlags.Public | BindingFlags.NonPublic | (isStatic ? BindingFlags.Static : BindingFlags.Instance);

			var prop = objType.GetProperty(name, flags);
			if (prop != null)
				return prop;

			var field = objType.GetField(name, flags);
			if (field != null)
				return field;

			return null;
		}

		/// <summary>
		/// 获取一批指定名字的属性或字段
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static MemberInfo[] GetDataMemebers(Type objType, string[] names, bool isStatic)
		{
			if (objType == null || names == null)
				return null;

			int count = names.Length;
			var result = new MemberInfo[count];

			for (int i = 0; i < count; ++i)
				result[i] = GetDataMember(objType, names[i], isStatic);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static object ChangeType(object value, Type conversionType)
		{
			if (value != null && value.GetType() == conversionType)
				return value;

			return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// 通过反射为数据变量赋值
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetValue(MemberInfo member, object value, object instanceObj)
		{
			if (member == null)
				return;

			switch (member.MemberType)
			{
				case MemberTypes.Property:
					var prop = (PropertyInfo)member;
					prop.SetValue(instanceObj, ChangeType(value, prop.PropertyType), null);
					break;
				case MemberTypes.Field:
					var field = (FieldInfo)member;
					field.SetValue(instanceObj, ChangeType(value, field.FieldType));
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// 通过反射为数据变量赋值
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetValue(Type objType, string name, object value, object instanceObj)
		{
			if (objType == null || name == null)
				return;

			bool isStatic = (instanceObj == null);

			var member = GetDataMember(objType, name, isStatic);
			if (member != null)
				SetValue(member, value, instanceObj);
		}

		/// <summary>
		/// 获取首个指定类型Attribute
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetFirstAttribute<T>(this MemberInfo element, bool inherit) where T : Attribute
		{
			var attributes = element.GetCustomAttributes(typeof(T), inherit);
			if (attributes == null || attributes.Length <= 0)
				return null;

			return (T)attributes[0];
		}

		public static readonly string[] SystemAssemblyPrefixList = new string[] {
			"mscorlib","netstandard","System.", "Mono.","Microsoft.", "Unity.","UnityEngine.","UnityEditor.",
		};

		/// <summary>
		/// 检查是否用户程序集
		/// (简单过滤是否在SystemAssemblyPrefixList内)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsUserAssembly(Assembly assembly)
		{
			var name = assembly.FullName;

			foreach (var prefix in SystemAssemblyPrefixList)
			{
				if (name.StartsWith(prefix))
					return false;
			}

			return true;
		}

		/// <summary>
		/// 获取可加载类型
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
		{
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				// Gets the array of classes that were defined in the module and loaded.
				return e.Types.Where(t => t != null);
			}
		}

		/// <summary>
		/// 获取所有类型定义，允许指定只收集用户程序级
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<Type> GetAllTypes(bool userAssemblyOnly = true)
		{
			return AppDomain.CurrentDomain.GetAssemblies()
					.Where(asm => !userAssemblyOnly || IsUserAssembly(asm))
					.SelectMany(asm => asm.GetLoadableTypes());
		}
	}
}
