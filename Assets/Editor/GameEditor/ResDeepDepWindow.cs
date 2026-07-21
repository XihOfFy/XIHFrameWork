using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class ResDeepDepWindow : EditorWindow
{
    const string ResPath = "Assets/Res";
    const string AssetsRoot = "Assets/";

    [SerializeField]
    public class ResInfo
    {
        public Object sourceObj;
        public Object obj;
        public string path;
        public int depth;
        public bool isCycle;
    }

    ReorderableList sourceListView;
    ReorderableList depListView;
    List<Object> sourceList;
    List<ResInfo> depList;
    HashSet<string> depPathSet;
    Vector2 scrollPosition;

    [MenuItem("XIHUtil/ResDep/ResDeepDepWindow")]
    static void OpenWindow()
    {
        GetWindow<ResDeepDepWindow>();
    }

    private void OnEnable()
    {
        RefreshParamTable();
    }

    void RefreshParamTable()
    {
        if (sourceList == null) sourceList = new List<Object>();
        if (depList == null) depList = new List<ResInfo>();
        if (depPathSet == null) depPathSet = new HashSet<string>();

        sourceListView = new ReorderableList(sourceList, typeof(Object));
        sourceListView.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "原始资源");
        sourceListView.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            if (index >= sourceList.Count) return;
            sourceList[index] = EditorGUI.ObjectField(rect, sourceList[index], typeof(Object), false);
        };

        depListView = new ReorderableList(depList, typeof(ResInfo));
        depListView.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "深度依赖");
        depListView.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            if (index >= depList.Count) return;

            var info = depList[index];
            var sourceRect = rect;
            sourceRect.width = rect.width * 0.22f;
            EditorGUI.ObjectField(sourceRect, info.sourceObj, typeof(Object), false);

            var objRect = rect;
            objRect.x = sourceRect.xMax + 6;
            objRect.width = rect.width * 0.28f;
            EditorGUI.ObjectField(objRect, info.obj, typeof(Object), false);

            var depthRect = rect;
            depthRect.x = objRect.xMax + 6;
            depthRect.width = 56;
            EditorGUI.LabelField(depthRect, $"层级:{info.depth}");

            var pathRect = rect;
            pathRect.x = depthRect.xMax + 6;
            pathRect.width = rect.xMax - pathRect.x;
            EditorGUI.LabelField(pathRect, info.isCycle ? $"{info.path}  (循环依赖)" : info.path);
        };
        depListView.drawFooterCallback = rect => { };
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("添加编辑器选中的所有资源到列表"))
        {
            AddObjects(Selection.objects);
        }
        if (GUILayout.Button("添加打包配置(PackGroup)所有资源"))
        {
            AddPackGroupAssets();
        }
        if (GUILayout.Button("清空列表"))
        {
            sourceList.Clear();
            depList.Clear();
            depPathSet.Clear();
        }
        EditorGUILayout.EndHorizontal();

        DrawDropArea();
        sourceListView.DoLayoutList();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("查询深度依赖"))
        {
            SearchDeepDependencies();
        }
        if (GUILayout.Button("输出所有依赖和原始资源路径"))
        {
            OutputAllAssetPaths();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField($"依赖数量: {depList.Count}");
        depListView.DoLayoutList();

        EditorGUILayout.EndScrollView();
    }

    void DrawDropArea()
    {
        var rect = GUILayoutUtility.GetRect(0, 42, GUILayout.ExpandWidth(true));
        GUI.Box(rect, "拖拽原始资源到这里");

        var evt = Event.current;
        if (!rect.Contains(evt.mousePosition))
            return;

        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                AddObjects(DragAndDrop.objectReferences);
            }
            evt.Use();
        }
    }

    void AddObjects(IEnumerable<Object> objects)
    {
        foreach (var obj in objects)
        {
            if (obj == null)
                continue;

            string path = AssetDatabase.GetAssetPath(obj);
            if (!IsAssetPath(path))
                continue;

            if (!sourceList.Contains(obj))
                sourceList.Add(obj);
        }
    }

    // 将 PackUtil.PackGroup 配置的所有资源批量加入原始资源列表,免去手动拖拽。
    // 用反射读取,避免 ResDeepDepWindow 与 YooSetting.Editor 程序集硬耦合(其 autoReferenced=false 且受 USE_YOO 约束)
    void AddPackGroupAssets()
    {
        var packGroup = GetPackGroup();
        if (packGroup == null)
        {
            Debug.LogWarning("[PackGroup] 未找到 PackUtil.PackGroup,请确认 YooSetting.Editor 程序集已编译(需定义 USE_YOO)");
            return;
        }

        var objs = new List<Object>();
        foreach (DictionaryEntry entry in packGroup)
        {
            if (!(entry.Value is IEnumerable list))
                continue;
            foreach (var element in list)
            {
                if (element is string item && !string.IsNullOrEmpty(item))
                    CollectPackItem(item, objs);
            }
        }

        AddObjects(objs);
        Debug.Log($"[PackGroup] 已添加,当前原始资源数量: {sourceList.Count}");
    }

    // 反射获取 PackUtil.PackGroup(无命名空间的顶层静态类),遍历所有已加载程序集查找
    static IDictionary GetPackGroup()
    {
        foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = asm.GetType("PackUtil");
            if (type == null)
                continue;
            var field = type.GetField("PackGroup", BindingFlags.Public | BindingFlags.Static);
            return field?.GetValue(null) as IDictionary;
        }
        return null;
    }

    // 单条配置项:以 / 结尾或本身是文件夹 → 收集目录下所有资源;否则按单个文件处理
    void CollectPackItem(string item, List<Object> objs)
    {
        var p = item.Replace('\\', '/');
        var folder = p.TrimEnd('/');
        if (p.EndsWith("/") || AssetDatabase.IsValidFolder(folder))
        {
            var guids = AssetDatabase.FindAssets(string.Empty, new string[] { folder });
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!IsAssetPath(assetPath))
                    continue;
                var obj = AssetDatabase.LoadMainAssetAtPath(assetPath);
                if (obj != null)
                    objs.Add(obj);
            }
        }
        else
        {
            var obj = AssetDatabase.LoadMainAssetAtPath(p);
            if (obj != null)
                objs.Add(obj);
            else
                Debug.LogWarning($"[PackGroup] 资源不存在或路径无效: {p}");
        }
    }

    void SearchDeepDependencies()
    {
        depList.Clear();
        depPathSet.Clear();

        foreach (var sourceObj in sourceList)
        {
            if (sourceObj == null)
                continue;

            string sourcePath = AssetDatabase.GetAssetPath(sourceObj);
            if (!IsAssetPath(sourcePath))
                continue;

            SearchDeepDependencies(sourceObj, sourcePath, new HashSet<string>(), new HashSet<string>(), 1);
        }
    }

    void SearchDeepDependencies(Object sourceObj, string path, HashSet<string> visitedPaths, HashSet<string> recursionPaths, int depth)
    {
        visitedPaths.Add(path);
        recursionPaths.Add(path);

        string[] dependencies = AssetDatabase.GetDependencies(path, false)
            .Where(IsAssetPath)
            .OrderBy(p => p)
            .ToArray();

        foreach (string dependencyPath in dependencies)
        {
            if (dependencyPath == path)
                continue;

            bool isCycle = recursionPaths.Contains(dependencyPath);
            string depKey = $"{AssetDatabase.GetAssetPath(sourceObj)}|{dependencyPath}";
            if (!depPathSet.Contains(depKey))
            {
                depPathSet.Add(depKey);
                depList.Add(new ResInfo
                {
                    sourceObj = sourceObj,
                    obj = AssetDatabase.LoadMainAssetAtPath(dependencyPath),
                    path = dependencyPath,
                    depth = depth,
                    isCycle = isCycle
                });
            }

            if (isCycle || visitedPaths.Contains(dependencyPath))
                continue;

            SearchDeepDependencies(sourceObj, dependencyPath, visitedPaths, recursionPaths, depth + 1);
        }

        recursionPaths.Remove(path);
    }

    void OutputAllAssetPaths()
    {
        var pathSet = new HashSet<string>();
        foreach (var obj in sourceList)
        {
            if (obj == null)
                continue;

            string path = AssetDatabase.GetAssetPath(obj);
            if (IsAssetPath(path) && !IsExcludedOutputPath(path))
                pathSet.Add(path);
        }

        foreach (var info in depList)
        {
            if (IsAssetPath(info.path) && !IsExcludedOutputPath(info.path))
                pathSet.Add(info.path);
        }

        Debug.Log(string.Join(",\n", pathSet.OrderBy(s => s).Select(s => $"\"{s}\"")));
    }

    // 输出时剔除脚本(.cs)和场景(.unity)资源
    static bool IsExcludedOutputPath(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext == ".cs" || ext == ".unity";
    }

    static bool IsAssetPath(string path)
    {
        return !string.IsNullOrEmpty(path)
            && path.StartsWith(AssetsRoot)
            && !Directory.Exists(path);
    }
}
