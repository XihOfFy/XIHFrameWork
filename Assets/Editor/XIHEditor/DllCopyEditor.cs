using HybridCLR.Editor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DllCopyEditor
{
    [MenuItem("XIHUtil/Hclr/CopyAndroidDlls")]
    static void CopyAndroidDlls()
    {
        CopyDlls(BuildTarget.Android);
    }
    [MenuItem("XIHUtil/Hclr/CopyiOSDlls")]
    static void CopyiOSDlls()
    {
        CopyDlls(BuildTarget.iOS);
    }
    [MenuItem("XIHUtil/Hclr/CopyWebglDlls")]
    static void CopyWebglDlls()
    {
        CopyDlls(BuildTarget.WebGL);
    }
    public static void CopyDlls(BuildTarget target)
    {
        CopyAotDll("Assets/Res/Raw/Aot", target);
        CopyHotDll(true, "Assets/Res/Aot2Hot/Raw", target);
        CopyHotDll(false, "Assets/Res/Raw/Hot", target);
        AssetDatabase.Refresh();
    }
    static void CopyAotDll(string dstPath, BuildTarget target)
    {
        List<string> dlls = SettingsUtil.AOTAssemblyNames;
        string sourPath = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
        if (!Directory.Exists(sourPath))
        {
            Debug.LogWarning($"{sourPath}路径不存在，请先编译dll");
            return;
        }
        if (Directory.Exists(dstPath)) Directory.Delete(dstPath, true);
        Directory.CreateDirectory(dstPath);
        foreach (var dll in dlls)
        {
            var ph = $"{sourPath}/{dll}.dll";
            if (!File.Exists(ph))
            {
                Debug.LogError($"CopyDll {ph}不存在");
                continue;
            }
            File.Copy(ph, $"{dstPath}/{dll}.bytes");
        }
        Debug.LogWarning($"拷贝无需加密的Aotdlls到 {dstPath}");
    }
    static void CopyHotDll(bool isAot2Hot, string dstPath, BuildTarget target)
    {
        var aot2hotdll = new HashSet<string>() { "Aot2Hot" };
        List<string> dlls = null;
        if (isAot2Hot)
        {
            dlls = aot2hotdll.ToList();
        }
        else
        {
            var otherHotdll = new HashSet<string>(SettingsUtil.HotUpdateAssemblyNamesIncludePreserved);
            foreach (var dll in aot2hotdll) otherHotdll.Remove(dll);
            dlls = otherHotdll.ToList();
        }
        var sourPath = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
        if (!Directory.Exists(sourPath))
        {
            Debug.LogWarning($"{sourPath}路径不存在，请先编译dll");
            return;
        }
        if (Directory.Exists(dstPath)) Directory.Delete(dstPath, true);
        Directory.CreateDirectory(dstPath);
        foreach (var dll in dlls)
        {
            XIHEncryptionServices.Encrypt($"{sourPath}/{dll}.dll", $"{dstPath}/{dll}.bytes");
        }
        Debug.LogWarning($"拷贝加密的dlls到 {dstPath}");
    }
}
