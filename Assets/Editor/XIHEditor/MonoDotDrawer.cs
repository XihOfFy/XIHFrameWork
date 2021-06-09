
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Intepreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace XIHBasic
{
    /// <summary>
    /// 简单运行时展示HotFix中的类成员，需要修改的可以自行扩展
    /// </summary>
    [CustomEditor(typeof(MonoDotBase))]
    public class MonoDotDrawer : Editor
    {
        ILTypeInstance instance;
        IEnumerable<FieldInfo> fields;
        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            var dot = serializedObject.targetObject as MonoDotBase;
            var hotfixTypeName = dot.hotFixTypeName;
            var type = dot.GetType();
            var field = type.GetField("instance", BindingFlags.Instance | BindingFlags.NonPublic);
            instance = field.GetValue(dot) as ILTypeInstance;
            IType t = HotFixBridge.Appdomain.LoadedTypes[hotfixTypeName];
            type = t.ReflectionType;
            var strStr = typeof(string).ToString();
            fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public).Where(fi => fi.FieldType.IsPrimitive || fi.FieldType.IsEnum || fi.FieldType.ToString() == strStr);//属性仅get set会有隐藏私有成员
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            if (Application.isPlaying)
            {
                EditorGUILayout.BeginVertical();
                foreach (var field in fields)
                {
                    EditorGUILayout.LabelField($"{field.Name}:{field.GetValue(instance)}");
                }
                EditorGUILayout.EndVertical();
            }
        }
    }
}