#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Hot
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 禁用 GUI，使其不可编辑
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            //GUI.enabled = true;
        }
    }
}
#endif