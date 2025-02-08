using Aot;
using System.IO;
using UnityEditor;
using YooAsset;

public class YooEditorSimulateModeHelperWindow : EditorWindow
{
    [MenuItem("YooAsset/YooEditorSimulateModeHelperBuild (耗时)")]
    static void OpenWindow() {
        if (EditorUtility.DisplayDialog("构建", "是否构建模拟资源包 (耗时2分钟以上)", "OK", "Cancel"))
        {
            var result = EditorSimulateModeHelper.SimulateBuild(AotConfig.PACKAGE_NAME);
            var dir = Path.GetDirectoryName(result.PackageRootDirectory);
            var dst = $"XIHWebServerRes/Bundles/{EditorUserBuildSettings.activeBuildTarget}/{AotConfig.PACKAGE_NAME}/Simulate";
            if(Directory.Exists(dst))Directory.Delete(dst, true);
            Directory.CreateDirectory(Path.GetDirectoryName(dst));
            Directory.Move(dir, dst);
        }
    }
}
