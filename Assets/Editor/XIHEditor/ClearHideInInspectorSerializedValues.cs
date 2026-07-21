using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 将 MonoBehaviour 上标记了 [HideInInspector] 的序列化字段重置为默认值。
/// 解决：原为 public 在预制体上配了值，后改为 [HideInInspector] 后 Inspector 不显示但 YAML 仍保留旧引用/数值的问题。
/// </summary>
public static class ClearHideInInspectorSerializedValues
{
    const string MenuRoot = "Tools/预制体/清除 HideInInspector 序列化值";

    [MenuItem(MenuRoot + "/当前场景与层级选中物体")]
    static void ClearSelectionInHierarchy()
    {
        var roots = Selection.gameObjects;
        if (roots == null || roots.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "请在 Hierarchy 中选中一个或多个物体。", "确定");
            return;
        }

        Undo.IncrementCurrentGroup();
        var count = 0;
        foreach (var go in roots)
        {
            if (!go) continue;
            count += ProcessHierarchy(go, true);
        }
        Undo.SetCurrentGroupName("Clear HideInInspector Fields");
        AssetDatabase.SaveAssets();
        Debug.Log($"[ClearHideInInspector] 已处理组件字段次数: {count}（层级选中）");
    }

    [MenuItem(MenuRoot + "/Project 中选中的预制体资源")]
    static void ClearSelectedPrefabAssets()
    {
        var paths = CollectSelectedPrefabPaths();
        if (paths.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "请在 Project 中选中一个或多个 .prefab 资源。", "确定");
            return;
        }

        if (!EditorUtility.DisplayDialog("确认", $"将对 {paths.Count} 个预制体根层级执行清除，是否继续？", "继续", "取消"))
            return;

        var total = 0;
        try
        {
            for (var i = 0; i < paths.Count; i++)
            {
                var path = paths[i];
                EditorUtility.DisplayProgressBar("清除 HideInInspector 字段", path, (float)i / paths.Count);
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    total += ProcessHierarchy(root, false);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[ClearHideInInspector] 已处理组件字段次数: {total}（预制体资源 {paths.Count} 个）");
    }

    static List<string> CollectSelectedPrefabPaths()
    {
        var list = new List<string>();
        foreach (var obj in Selection.objects)
        {
            if (obj is not GameObject) continue;
            var path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) continue;
            if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                list.Add(path);
        }
        return list;
    }

    /// <summary>返回被重置的字段数量（每个字段计 1）。</summary>
    static int ProcessHierarchy(GameObject root, bool recordUndo)
    {
        var count = 0;
        var transforms = root.GetComponentsInChildren<Transform>(true);
        foreach (var tr in transforms)
        {
            var mbs = tr.GetComponents<MonoBehaviour>();
            foreach (var mb in mbs)
            {
                if (!mb) continue;
                count += ClearHideFieldsOnBehaviour(mb, recordUndo);
            }
        }
        return count;
    }

    static int ClearHideFieldsOnBehaviour(MonoBehaviour mb, bool recordUndo)
    {
        if (recordUndo)
            Undo.RecordObject(mb, "Clear HideInInspector Fields");

        var so = new SerializedObject(mb);
        so.Update();
        var cleared = 0;

        for (var t = mb.GetType(); t != null && t != typeof(MonoBehaviour) && t != typeof(Component); t = t.BaseType)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            foreach (var field in t.GetFields(flags))
            {
                if (!IsUnitySerializedField(field)) continue;
                if (!field.IsDefined(typeof(HideInInspector), false)) continue;

                var prop = so.FindProperty(field.Name);
                if (prop == null) continue;

                ClearPropertyRecursive(prop, field.FieldType);
                cleared++;
            }
        }

        if (cleared > 0)
        {
            so.ApplyModifiedProperties();
            if (!recordUndo)
                EditorUtility.SetDirty(mb);
        }

        return cleared;
    }

    static bool IsUnitySerializedField(FieldInfo field)
    {
        if (field.IsStatic) return false;
        if (field.IsDefined(typeof(NonSerializedAttribute), false)) return false;
        if (field.IsPublic) return true;
        return field.IsDefined(typeof(SerializeField), false);
    }

    static void ClearPropertyRecursive(SerializedProperty prop, Type fieldType)
    {
        // 数组 / List
        if (prop.isArray && prop.propertyType != SerializedPropertyType.String)
        {
            if (prop.arraySize > 0)
                prop.arraySize = 0;
            return;
        }
        Debug.Log($"ClearPropertyRecursive: {prop.propertyType} {prop.name}");
        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer:
            case SerializedPropertyType.LayerMask:
                prop.intValue = 0;
                return;
            case SerializedPropertyType.Boolean:
                prop.boolValue = false;
                return;
            case SerializedPropertyType.Float:
                prop.floatValue = 0f;
                return;
            case SerializedPropertyType.String:
                prop.stringValue = string.Empty;
                return;
            case SerializedPropertyType.Color:
                prop.colorValue = default;
                return;
            case SerializedPropertyType.ObjectReference:
                prop.objectReferenceValue = null;
                return;
            case SerializedPropertyType.Enum:
                if (prop.enumNames != null && prop.enumNames.Length > 0)
                    prop.enumValueIndex = 0;
                return;
            case SerializedPropertyType.Vector2:
                prop.vector2Value = Vector2.zero;
                return;
            case SerializedPropertyType.Vector3:
                prop.vector3Value = Vector3.zero;
                return;
            case SerializedPropertyType.Vector4:
                prop.vector4Value = Vector4.zero;
                return;
            case SerializedPropertyType.Rect:
                prop.rectValue = default;
                return;
            case SerializedPropertyType.Bounds:
                prop.boundsValue = default;
                return;
            case SerializedPropertyType.Quaternion:
                prop.quaternionValue = Quaternion.identity;
                return;
            case SerializedPropertyType.AnimationCurve:
                prop.animationCurveValue = null;
                return;
            case SerializedPropertyType.ManagedReference:
                prop.managedReferenceValue = null;
                return;
            case SerializedPropertyType.Character:
                prop.intValue = 0;
                return;
            case SerializedPropertyType.Generic:
                if (!prop.hasChildren)
                    return;
                var end = prop.GetEndProperty();
                var it = prop.Copy();
                if (!it.Next(true))
                    return;
                do
                {
                    if (SerializedProperty.EqualContents(it, end))
                        break;
                    ClearPropertyRecursive(it, null);
                } while (it.Next(false));
                return;
            default:
                return;
        }
    }
}
