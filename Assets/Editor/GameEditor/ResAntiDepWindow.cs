using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class ResAntiDepWindow : EditorWindow
{
    const string ResPath = "Assets/Res";
    const string AssetsRoot = "Assets/";

    enum UnusedAssetType
    {
        Texture,
        Sprite,
        Material,
        Mesh,
        Model
    }

    static readonly string[] ModelExtensions = { ".fbx", ".obj", ".dae", ".3ds", ".dxf", ".blend", ".mb", ".ma" };

    [MenuItem("XIHUtil/ResDep/ResAntiDepWindow")]
    static void OpenWindow()
    {
        GetWindow<ResAntiDepWindow>();
    }
    [SerializeField]
    public class ResInfo
    {
        public Object obj;
        public string path;
        public Object depObj;
    }

    ReorderableList listView;
    List<Object> choosedList;
    Vector2 m_ScrollPosition;
    List<ResInfo> resList;
    ReorderableList resListView;
    List<Object> unusedAssetList;
    ReorderableList unusedAssetListView;
    [SerializeField] UnusedAssetType unusedAssetType = UnusedAssetType.Texture;

    private static Dictionary<string, string> pathToGuidMap = new Dictionary<string, string>();
    private static Dictionary<string, List<string>> reverseDependencyMap = new Dictionary<string, List<string>>();
    private static bool dependencyDataReady;

    private void OnEnable()
    {
        dependencyDataReady = false;
        RefreshParamTable();
    }
    void RefreshParamTable()
    {
        choosedList = new List<Object>();
        listView = new ReorderableList(choosedList, typeof(Object));
        listView.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "资源");
        listView.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            if (index >= choosedList.Count) return;
            var obj = choosedList[index];
            choosedList[index] = EditorGUI.ObjectField(rect, obj, typeof(Object), false);
        };
        listView.onCanAddCallback = (list) =>
        {
            return true;
        };
        listView.onCanRemoveCallback = (list) =>
        {
            return true;
        };
        //listView.drawFooterCallback = rect => { };

        m_ScrollPosition = Vector2.zero;

        resList = new List<ResInfo>();
        resListView = new ReorderableList(resList, typeof(ResInfo));
        resListView.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "依赖");
        resListView.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            if (index >= resList.Count) return;
            var obj = resList[index];
            rect.width /= 2;
            EditorGUI.ObjectField(rect, obj.obj, typeof(Object), false);
            rect.x += rect.width;
            EditorGUI.ObjectField(rect, "依赖了", obj.depObj, typeof(Object), false);
            //EditorGUI.LabelField(rect, obj.path);
        };
        resListView.drawFooterCallback = rect => { };

        unusedAssetList = new List<Object>();
        unusedAssetListView = new ReorderableList(unusedAssetList, typeof(Object));
        unusedAssetListView.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "未引用资源");
        unusedAssetListView.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            if (index >= unusedAssetList.Count) return;
            unusedAssetList[index] = EditorGUI.ObjectField(rect, unusedAssetList[index], typeof(Object), false);
        };
        unusedAssetListView.drawFooterCallback = rect => { };
    }
    private void RefreshData()
    {
        dependencyDataReady = false;
        var dependencyMap = new Dictionary<string, HashSet<string>>();
        reverseDependencyMap.Clear();
        pathToGuidMap.Clear();

        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths()
            .Where(IsScannableAssetPath)
            .ToArray();

        // 创建路径到GUID的映射
        foreach (string path in allAssetPaths)
        {
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (!string.IsNullOrEmpty(guid))
            {
                pathToGuidMap[path] = guid;
            }
        }

        // 分析依赖关系
        int total = allAssetPaths.Length;
        bool isCanceled = false;
        for (int i = 0; i < total; i++)
        {
            string path = allAssetPaths[i];
            if (EditorUtility.DisplayCancelableProgressBar("分析依赖关系", $"处理: {Path.GetFileName(path)}", (float)i / total))
            {
                isCanceled = true;
                break;
            }

            string guid = pathToGuidMap[path];
            if (!dependencyMap.ContainsKey(guid))
            {
                dependencyMap[guid] = new HashSet<string>();
            }

            // 获取所有依赖
            string[] dependencies = AssetDatabase.GetDependencies(path, true);
            foreach (string dependencyPath in dependencies)
            {
                // 排除自身和脚本文件
                if (dependencyPath == path || dependencyPath.EndsWith(".cs"))
                    continue;

                if (pathToGuidMap.TryGetValue(dependencyPath, out string dependencyGuid))
                {
                    if (dependencyGuid != guid)
                    {
                        dependencyMap[guid].Add(dependencyGuid);
                    }
                }
            }
        }

        EditorUtility.ClearProgressBar();
        if (isCanceled)
            return;

        var tmpReverseDependencyMap = new Dictionary<string, HashSet<string>>();
        foreach (var kv in dependencyMap)
        {
            var depGuid = kv.Key;
            var vals = kv.Value;
            foreach (var val in vals)
            {
                if (!tmpReverseDependencyMap.TryGetValue(val, out var list))
                {
                    list = new HashSet<string>();
                    tmpReverseDependencyMap[val] = list;
                }
                list.Add(depGuid);
            }
        }
        foreach (var kv in tmpReverseDependencyMap)
        {
            reverseDependencyMap[kv.Key] = kv.Value.ToList();
        }
        dependencyDataReady = true;
    }

    private void OnGUI()
    {
        if (GUILayout.Button("初始化资源依赖"))
        {
            RefreshData();
        }
        m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("添加编辑器选中的所有资源到列表"))
        {
            foreach (var obj in Selection.objects)
                if (!choosedList.Contains(obj))
                    choosedList.Add(obj);
        }
        if (GUILayout.Button("清空列表"))
        {
            choosedList.Clear();
        }
        EditorGUILayout.EndHorizontal();
        listView.DoLayoutList();
        if (GUILayout.Button("检查以上所有资源的引用"))
        {
            EnsureDependencyData();
            CheckResReference();
            /*var resDic = CheckResReference();
            resList.Clear();
            foreach (var kv in resDic)
            {
                foreach (var vs in kv.Value) resList.Add(new ResInfo() { obj = vs, path = vs.name });
            }*/
        }
        if (GUILayout.Button("输出所有被依赖资源所属的Res下2层级的根路径"))
        {
            var set = new HashSet<string>(resList.Count);
            foreach (var aset in resList)
            {
                var path = AssetDatabase.GetAssetPath(aset.obj);
                path = path.Substring("Assets/Res/".Length);
                var index = path.IndexOf('/');
                var root = path.Substring(0, index);
                path = path.Substring(index + 1);
                index = path.IndexOf('/');
                if (index != -1)
                {
                    root += "/" + path.Substring(0, index);
                }
                var prePath = "Assets/Res/" + root + "/";
                set.Add(prePath);
            }
            foreach (var aset in choosedList)
            {
                var path = AssetDatabase.GetAssetPath(aset);
                path = path.Substring("Assets/Res/".Length);
                var index = path.IndexOf('/');
                var root = path.Substring(0, index);
                path = path.Substring(index + 1);
                index = path.IndexOf('/');
                if (index != -1)
                {
                    root += "/" + path.Substring(0, index);
                }
                var prePath = "Assets/Res/" + root + "/";
                set.Add(prePath);
            }
            Debug.Log(string.Join(",\n", set.Select(s => $"\"{s}\"")));
        }
        GUILayout.Space(50);
        EditorGUILayout.LabelField("未引用资源检查", EditorStyles.boldLabel);
        unusedAssetType = (UnusedAssetType)EditorGUILayout.EnumPopup("资源类型", unusedAssetType);
        if (GUILayout.Button("一键检查所选类型未引用资源"))
        {
            RefreshData();
            CheckUnusedAssetsByType(unusedAssetType);
        }
        EditorGUILayout.LabelField($"结果数量: {unusedAssetList.Count}");
        unusedAssetListView.DoLayoutList();
        resListView.DoLayoutList();
        EditorGUILayout.EndScrollView();
    }
    void HighlightAsset(Object obj)
    {
        // 加载资源并高亮显示
        //Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (obj != null)
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }
    }

    void CheckResReference()
    {
        resList.Clear();
        foreach (var obj in choosedList)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (reverseDependencyMap.TryGetValue(guid, out var list))
            {
                //var res = new List<ResInfo>();
                //resDic[obj] = res;
                foreach (var item in list)
                {
                    path = AssetDatabase.GUIDToAssetPath(item);
                    var ast = AssetDatabase.LoadAssetAtPath<Object>(path);
                    //res.Add(new ResInfo() { obj = ast, depObj = obj });
                    resList.Add(new ResInfo() { obj = ast, depObj = obj });
                }
            }
        }
        //return resDic;
    }

    void EnsureDependencyData()
    {
        if (!dependencyDataReady)
        {
            RefreshData();
        }
    }

    void CheckUnusedAssetsByType(UnusedAssetType checkType)
    {
        unusedAssetList.Clear();
        var candidatePaths = GetCandidateAssetPaths(checkType);
        int total = candidatePaths.Count;
        for (int i = 0; i < total; i++)
        {
            string path = candidatePaths[i];
            if (EditorUtility.DisplayCancelableProgressBar("检查未引用资源", $"处理: {Path.GetFileName(path)}", total == 0 ? 1f : (float)i / total))
            {
                break;
            }

            string guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid))
                continue;

            if (reverseDependencyMap.TryGetValue(guid, out var refs) && refs.Count > 0)
                continue;

            Object asset = LoadDisplayObject(path, checkType);
            if (asset != null)
            {
                unusedAssetList.Add(asset);
            }
        }

        EditorUtility.ClearProgressBar();
    }

    static bool IsScannableAssetPath(string path)
    {
        if (string.IsNullOrEmpty(path) || !path.StartsWith(AssetsRoot) || Directory.Exists(path))
            return false;

        string ext = Path.GetExtension(path).ToLowerInvariant();
        return ext != ".cs" && ext != ".meta";
    }

    List<string> GetCandidateAssetPaths(UnusedAssetType checkType)
    {
        return AssetDatabase.GetAllAssetPaths()
            .Where(p => p.StartsWith(ResPath) && !Directory.Exists(p) && IsTargetAssetType(p, checkType))
            .OrderBy(p => p)
            .ToList();
    }

    static bool IsTargetAssetType(string path, UnusedAssetType checkType)
    {
        switch (checkType)
        {
            case UnusedAssetType.Texture:
                return IsTextureAsset(path, false);
            case UnusedAssetType.Sprite:
                return IsTextureAsset(path, true);
            case UnusedAssetType.Material:
                return AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(Material);
            case UnusedAssetType.Mesh:
                return IsMeshAsset(path);
            case UnusedAssetType.Model:
                return IsModelAsset(path);
            default:
                return false;
        }
    }

    static bool IsTextureAsset(string path, bool spriteOnly)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            return false;

        bool isSprite = importer.textureType == TextureImporterType.Sprite;
        return spriteOnly ? isSprite : !isSprite;
    }

    static bool IsMeshAsset(string path)
    {
        if (AssetImporter.GetAtPath(path) is ModelImporter)
            return false;

        var mainType = AssetDatabase.GetMainAssetTypeAtPath(path);
        return mainType == typeof(Mesh);
    }

    static bool IsModelAsset(string path)
    {
        if (AssetImporter.GetAtPath(path) is ModelImporter)
            return true;

        string ext = Path.GetExtension(path).ToLowerInvariant();
        return ModelExtensions.Contains(ext);
    }

    static Object LoadDisplayObject(string path, UnusedAssetType checkType)
    {
        switch (checkType)
        {
            case UnusedAssetType.Sprite:
                return AssetDatabase.LoadAllAssetsAtPath(path).FirstOrDefault(o => o is Sprite) ?? AssetDatabase.LoadMainAssetAtPath(path);
            case UnusedAssetType.Mesh:
                return AssetDatabase.LoadAllAssetsAtPath(path).FirstOrDefault(o => o is Mesh) ?? AssetDatabase.LoadMainAssetAtPath(path);
            default:
                return AssetDatabase.LoadMainAssetAtPath(path);
        }
    }

    //[MenuItem("MolaUtil/检查图片引用 &s")]
    public static Dictionary<Object, List<Object>> CheckResReference(List<Object> choosedList)
    {
        /*        //清除以前的Log
                Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
                Type type = assembly.GetType("UnityEditor.LogEntries");
                MethodInfo method = type.GetMethod("Clear");
                method.Invoke(new object(), null);

                Debug.ClearDeveloperConsole();*/
        //Debug.Log("开始匹配");

        List<string> withoutExtensions = new List<string>() { ".prefab", ".unity", ".mat", ".asset", ".anim", ".controller" };
        string[] files = Directory.GetFiles(ResPath, "*.*", SearchOption.AllDirectories)
            .Where(s => withoutExtensions.Contains(System.IO.Path.GetExtension(s).ToLower())).ToArray();

        var total = files.Length * choosedList.Count;
        var resDic = new Dictionary<Object, List<Object>>();
        foreach (var obj in choosedList)
        {
            if (obj == null) continue;
            var res = new List<Object>();
            resDic[obj] = res;
            //res.Add(obj);
            int referenceFileCount = 0;
            string path = AssetDatabase.GetAssetPath(obj);
            string guid = AssetDatabase.AssetPathToGUID(path);
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                if (i % 20 == 0)
                {
                    bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, (float)i / total);
                    if (isCancel)
                    {
                        Debug.Log("匹配取消");
                        EditorUtility.ClearProgressBar();
                        return resDic;
                    }
                }
                if (Regex.IsMatch(File.ReadAllText(file), guid))
                {
                    //string[] fileName = file.Split(new string[]{ "Art" }, StringSplitOptions.None);
                    referenceFileCount++;
                    res.Add(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(file));
                }
            }
        }
        Debug.Log("匹配结束");
        EditorUtility.ClearProgressBar();
        return resDic;
    }
}
