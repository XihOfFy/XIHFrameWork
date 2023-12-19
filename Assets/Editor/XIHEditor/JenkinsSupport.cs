using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using YooAsset.Editor;
using Aot;
using System;
using System.IO;

//需要自行保证YooAsset资源收集规则已经提前设置了
public class JenkinsSupport 
{
    const string WEB_ROOT ="XIHWebServerRes";

    [MenuItem("XIHUtil/Jenkins/FullBuild")]
    public static void FullBuild()
    {
        var curTarget = EditorUserBuildSettings.activeBuildTarget;
        PrebuildCommand.GenerateAll();
        HotBuild();

        string targetPath = null;
        var buildOptions = BuildOptions.None;
        switch (curTarget)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                {
                    buildOptions = BuildOptions.None;

                    targetPath = $"{WEB_ROOT}/GameWin/";
                    if (Directory.Exists(targetPath)) Directory.Delete(targetPath, true);
                    Directory.CreateDirectory(targetPath);
                    targetPath += $"Game{PlayerSettings.bundleVersion}.exe";
                    break;
                }
            case BuildTarget.Android:
                {
                    buildOptions = BuildOptions.CompressWithLz4HC;
                    targetPath = $"{WEB_ROOT}/GameAndroid/";
                    if (Directory.Exists(targetPath)) Directory.Delete(targetPath, true);
                    Directory.CreateDirectory(targetPath);
                    targetPath += $"Game{PlayerSettings.bundleVersion}.apk";
                    break;
                }
            case BuildTarget.iOS:
                {
                    buildOptions = BuildOptions.CompressWithLz4HC;
                    targetPath = $"{WEB_ROOT}/GameiOS/";
                    if (Directory.Exists(targetPath)) Directory.Delete(targetPath, true);
                    Directory.CreateDirectory(targetPath);
                    targetPath += "Game";
                    break;
                }

            case BuildTarget.WebGL:
                {
                    buildOptions = BuildOptions.CompressWithLz4HC;
                    targetPath = $"{WEB_ROOT}/GameWebGL/";
                    if (Directory.Exists(targetPath)) Directory.Delete(targetPath, true);
                    Directory.CreateDirectory(targetPath);
                    Debug.LogError("后期是否需要支持微信自动化打包");
                    break;
                }
        }


        var report = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, targetPath, curTarget, buildOptions);
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Failed)
        {
            //若是无头模式构建，添加这个行
            //Environment.Exit(-1);
        }

    }
    [MenuItem("XIHUtil/Jenkins/HotBuild")]
    public static void HotBuild()
    {
        var curTarget = EditorUserBuildSettings.activeBuildTarget;
        Debug.LogWarning("开始热更构建");
        CompileDllCommand.CompileDll(curTarget);
        Debug.LogWarning("拷贝热更Dll");
        DllCopyEditor.CopyDlls(curTarget);
        if (ExecuteYooAssetBuild(curTarget)) {
            Debug.LogWarning("打包资源AB成功");
            var dstPath = $"{WEB_ROOT}/{curTarget}/StreamingAssets";
            if(Directory.Exists(dstPath))Directory.Delete(dstPath, true);
            var srcPath = "Assets/StreamingAssets";
            if (File.Exists($"{srcPath}.meta")) {
                File.Delete($"{srcPath}.meta");
            }
            Directory.Move(srcPath, dstPath);
            DeleteMetaFile(dstPath);
            Debug.LogWarning($"拷贝到目标目录:{srcPath} > {dstPath}");
        }
        Debug.LogWarning("完成热更打包");
        AssetDatabase.Refresh();
    }

    static bool ExecuteYooAssetBuild(BuildTarget buildTarget)
    {
        var PackageName = AotConfig.PACKAGE_NAME;
        var BuildPipeline = EBuildPipeline.ScriptableBuildPipeline;

        //var buildMode = AssetBundleBuilderSetting.GetPackageBuildMode(PackageName, BuildPipeline);
        var buildMode = EBuildMode.IncrementalBuild;
        //var fileNameStyle = AssetBundleBuilderSetting.GetPackageFileNameStyle(PackageName, BuildPipeline);
        var fileNameStyle = EFileNameStyle.BundleName_HashName;
        //var buildinFileCopyOption = AssetBundleBuilderSetting.GetPackageBuildinFileCopyOption(PackageName, BuildPipeline);
        var buildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyAll;
        //var buildinFileCopyParams = AssetBundleBuilderSetting.GetPackageBuildinFileCopyParams(PackageName, BuildPipeline);
        var buildinFileCopyParams = "";
        //var compressOption = AssetBundleBuilderSetting.GetPackageCompressOption(PackageName, BuildPipeline);
        var compressOption = ECompressOption.LZ4;

        ScriptableBuildParameters buildParameters = new ScriptableBuildParameters();
        buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
        buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
        buildParameters.BuildPipeline = BuildPipeline.ToString();
        buildParameters.BuildTarget = buildTarget;
        buildParameters.BuildMode = buildMode;
        buildParameters.PackageName = PackageName;
        buildParameters.PackageVersion = GetDefaultPackageVersion();
        buildParameters.VerifyBuildingResult = true;
        buildParameters.FileNameStyle = fileNameStyle;
        buildParameters.BuildinFileCopyOption = buildinFileCopyOption;
        buildParameters.BuildinFileCopyParams = buildinFileCopyParams;
        buildParameters.EncryptionServices = new EncryptionNone();
        buildParameters.CompressOption = compressOption;


        // 执行构建
        ScriptableBuildPipeline pipeline = new ScriptableBuildPipeline();
        var buildResult = pipeline.Run(buildParameters, true);
        if (buildResult.Success)
        {
            Debug.Log($"构建成功 : {buildResult.OutputPackageDirectory}");
            //EditorUtility.RevealInFinder(buildResult.OutputPackageDirectory);
        }
        else
        {
            Debug.LogError($"构建失败 : {buildResult.FailedTask}");
        }
        return buildResult.Success;
    }

    private static string GetDefaultPackageVersion()
    {
        int totalMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
        return DateTime.Now.ToString("yyyy-MM-dd") + "-" + totalMinutes;
    }

    static void DeleteMetaFile(string dst)
    {
        var mfs = Directory.GetFiles(dst, "*.meta");
        foreach (var file in mfs) File.Delete(file);

        var dirs = Directory.GetDirectories(dst);
        foreach (var dir in dirs) {
            DeleteMetaFile(dir);
        }
    }

    // 从构建命令里获取参数示例
    private static string GetBuildPackageName()
    {
        foreach (string arg in System.Environment.GetCommandLineArgs())
        {
            if (arg.StartsWith("buildPackage"))
                return arg.Split("="[0])[1];
        }
        return string.Empty;
    }

}
