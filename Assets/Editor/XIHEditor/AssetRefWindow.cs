using Aot.XiHUtil;
using Hot;
using System.Text;
using UnityEditor;
using UnityEngine;
using XiHUtil;
public class AssetRefWindow : EditorWindow
{
    [MenuItem("XIHUtil/AssetRef")]
    static void OpenWindow()
    {
        EditorWindow.GetWindow<AssetRefWindow>(true, "AssetRef");
    }
    StringBuilder str;
    string tip;
    Vector2 scrollPos;
    private void Awake()
    {
        str = new StringBuilder();
        tip = "";
    }
    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("刷新"))
        {
            AnalysicAssetRef();
        }
        if (GUILayout.Button("清空"))
        {
            str.Clear();
            tip = "";
        }
        EditorGUILayout.EndHorizontal();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.HelpBox(tip, MessageType.Info);
        EditorGUILayout.EndScrollView();
    }
    void AnalysicAssetRef()
    {
        if (str == null) str = new StringBuilder();
        str.Append(SyncTimeHelper.GetSystemTime());
        str.AppendLine("\n剩余资源引用个数\t 路径");
        foreach (var kv in AssetLoadUtil.AssetCacheDic)
        {
            str.AppendLine($"单:{kv.Value.refCount}\t {kv.Key}");
        }
        foreach (var kv in AssetLoadUtil.AssetAllCacheDic)
        {
            str.AppendLine($"组:{kv.Value.refCount}\t {kv.Key}");
        }
        str.AppendLine($"单资源个数 {AssetLoadUtil.AssetCacheDic.Count}\t 组资源个数 {AssetLoadUtil.AssetAllCacheDic.Count}");
        str.AppendLine();
        tip = str.ToString();
    }
}
