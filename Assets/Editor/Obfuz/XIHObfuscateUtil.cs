using HybridCLR.Editor.Commands;
using HybridCLR.Editor;
using Obfuz.Settings;
using Obfuz;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEditor;
using HybridCLR.Editor.Link;
using UnityEngine;
using HybridCLR.Editor.Meta;
using HybridCLR.Editor.Installer;
using Obfuz4HybridCLR;
using UnityEditor.Build;

public class XIHObfuscateUtil
{
    [MenuItem("XIHUtil/Obfuz&Hyclr/GenerateAll")]
    public static void GenerateAll()
    {
        //PrebuildCommandExt.GenerateAll(); 不使用，因为GenerateLinkXml 太一样

        var installer = new InstallerController();
        if (!installer.HasInstalledHybridCLR())
        {
            throw new BuildFailedException($"You have not initialized HybridCLR, please install it via menu 'HybridCLR/Installer'");
        }
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        ObfuscateUtil.CompileAndObfuscateHotUpdateAssemblies(target);
        Il2CppDefGeneratorCommand.GenerateIl2CppDef();

        //GenerateLinkXml();
        LinkGeneratorCommand.GenerateLinkXml(target);

        StripAOTDllCommand.GenerateStripedAOTDlls(target);
        MethodBridgeGeneratorCommand.GenerateMethodBridgeAndReversePInvokeWrapper(target);
        AOTReferenceGeneratorCommand.GenerateAOTGenericReference(target);
    }


    [MenuItem("HybridCLR/ObfuzExtension/GenerateLinkXmlForHybridCLR")]
    public static void GenerateLinkXml()
    {
        CompileDllCommand.CompileDllActiveBuildTarget();
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        var obfuzSettings = ObfuzSettings.Instance;

        var assemblySearchDirs = new List<string>
        {
            SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target),
        };
        ObfuscatorBuilder builder = ObfuscatorBuilder.FromObfuzSettings(obfuzSettings, target, true);
        builder.InsertTopPriorityAssemblySearchPaths(assemblySearchDirs);

        Obfuscator obfuz = builder.Build();
        obfuz.Run();


        List<string> hotfixAssemblies = SettingsUtil.HotUpdateAssemblyNamesExcludePreserved;

        var analyzer = new Analyzer(new PathAssemblyResolver(builder.CoreSettingsFacade.obfuscatedAssemblyOutputPath));
        var refTypes = analyzer.CollectRefs(hotfixAssemblies);

        // HyridCLR中 LinkXmlWritter不是public的，在其他程序集无法访问，只能通过反射操作
        var linkXmlWriter = typeof(SettingsUtil).Assembly.GetType("HybridCLR.Editor.Link.LinkXmlWriter");
        var writeMethod = linkXmlWriter.GetMethod("Write", BindingFlags.Public | BindingFlags.Instance);
        var instance = Activator.CreateInstance(linkXmlWriter);
        string linkXmlOutputPath = $"{Application.dataPath}/Obfuz/link.xml";
        writeMethod.Invoke(instance, new object[] { linkXmlOutputPath, refTypes });
        Debug.Log($"[GenerateLinkXmlForObfuscatedAssembly] output:{linkXmlOutputPath}");
        AssetDatabase.Refresh();
    }
}
