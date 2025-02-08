using Aot;
using System.IO;
using UnityEditor;
using UnityEngine;
using YooAsset;

public class YooEditorSimulateModeHelperWindow : EditorWindow
{
    [MenuItem("YooAsset/YooEditorSimulateModeHelperBuild (耗时)")]
    static void OpenWindow() {
        if (EditorUtility.DisplayDialog("构建", "是否构建模拟资源包", "OK", "Cancel"))
        {
            var result = EditorSimulateModeHelper.SimulateBuild(AotConfig.PACKAGE_NAME);
            var dir = result.PackageRootDirectory;
            var dst = AotMgr.GetYooEditorSimulateManifestPath(AotConfig.PACKAGE_NAME);
            if(Directory.Exists(dst))Directory.Delete(dst, true);
            Directory.CreateDirectory(Path.GetDirectoryName(dst));
            Directory.Move(dir, dst);
            Debug.Log($"Build Success\n{dir}\n{dst}");
        }
    }
}
