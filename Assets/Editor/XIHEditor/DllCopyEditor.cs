using HybridCLR.Editor;
using HybridCLR.Editor.HotUpdate;
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
        var bakupPath = dstPath + "~";
        try
        {
            CheckAccessMissingMetadata(bakupPath, target);
        }
        catch { 
        }
        if (Directory.Exists(bakupPath)) Directory.Delete(bakupPath, true);
        Directory.CreateDirectory(bakupPath);

        List<string> dlls = SettingsUtil.AOTAssemblyNames;
        string sourPath = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);//AOT 混淆后还是覆盖到这里
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
            File.Copy(ph, $"{bakupPath}/{dll}.dll",true);//先拷贝原始的到目标，方便下一次进行CheckAccessMissingMetadata
            HybridCLR.Editor.AOT.AOTAssemblyMetadataStripper.Strip(ph, $"{dstPath}/{dll}.bytes");
        }
        Debug.LogWarning($"拷贝无需加密的Aotdlls到 {dstPath}");
    }
    static void CheckAccessMissingMetadata(string aotDir, BuildTarget target)
    {
        // aotDir指向 构建主包时生成的裁剪aot dll目录，而不是最新的SettingsUtil.GetAssembliesPostIl2CppStripDir(target)目录。
        // 一般来说，发布热更新包时，由于中间可能调用过generate/all，SettingsUtil.GetAssembliesPostIl2CppStripDir(target)目录中包含了最新的aot dll，
        // 肯定无法检查出类型或者函数裁剪的问题。
        // 需要在构建完主包后，将当时的aot dll保存下来，供后面补充元数据或者裁剪检查。

        // 第2个参数excludeDllNames为要排除的aot dll。一般取空列表即可。对于旗舰版本用户，
        // excludeDllNames需要为dhe程序集列表，因为dhe 程序集会进行热更新，热更新代码中
        // 引用的dhe程序集中的类型或函数肯定存在。
        var checker = new MissingMetadataChecker(aotDir, new List<string>());
        string hotUpdateDir;
        if (Obfuz.Settings.ObfuzSettings.Instance.buildPipelineSettings.enable)
        {
            hotUpdateDir = Obfuz4HybridCLR.PrebuildCommandExt.GetObfuscatedHotUpdateAssemblyOutputPath(target);
        }
        else
        {
            hotUpdateDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
        }

        foreach (var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
        {
            string dllPath = $"{hotUpdateDir}/{dll}";
            bool notAnyMissing = checker.Check(dllPath);
            if (!notAnyMissing)
            {
                throw new UnityException("CheckAccessMissingMetadata:旧版元数据无法满足新的热更dll需求，需要重新出包！");
            }
        }
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
        string sourPath;
        if (Obfuz.Settings.ObfuzSettings.Instance.buildPipelineSettings.enable)
        {
            sourPath = Obfuz4HybridCLR.PrebuildCommandExt.GetObfuscatedHotUpdateAssemblyOutputPath(target);
        }
        else
        {
            sourPath = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
        }
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
