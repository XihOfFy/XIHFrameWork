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
using WeChatWASM;
using static WeChatWASM.WXConvertCore;

//需要自行保证YooAsset资源收集规则已经提前设置了
public class JenkinsSupport 
{
    //命令行参考: https://docs.unity.cn/cn/2021.1/Manual/CommandLineArguments.html
    //Jenkins安装了Unity插件的话，可以在Editor command line arguments 填写：
    //-quit -nographics -batchmode -buildTarget "${BuildTarget}" -projectPath "${ProjectRoot}" -executeMethod JenkinsSupport.JenkinsBuild buildID="${BUILD_ID}" fullBuild=${UseFullBuild} -logFile "${ProjectRoot}/JenkinsUnityBuildLog.log"

    const string WEB_ROOT ="XIHWebServerRes";

    public static void JenkinsBuild()
    {
        var dic = GetCommandLineArgs();
        if (dic.ContainsKey("fullBuild")) {
            var val = dic["fullBuild"];
            bool.TryParse(val, out var result);
            if (result)
            {
                FullBuild();
            }
            else {
                HotBuild();
            }
        }
    }

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
                    /*buildOptions = BuildOptions.CompressWithLz4HC;
                    targetPath = $"{WEB_ROOT}/GameWebGL/";
                    if (Directory.Exists(targetPath)) Directory.Delete(targetPath, true);
                    Directory.CreateDirectory(targetPath);*/
                    if (WXExportError.SUCCEED != WXConvertCore.DoExport()) {
                        //若是无头模式构建，添加这个行
                        //Environment.Exit(-1);
                    }
                    return;
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
            Directory.CreateDirectory(dstPath);
            var srcPath = "Assets/StreamingAssets";
            CopyDirs(srcPath,dstPath,".meta");
            Debug.LogWarning($"拷贝到目标目录:{srcPath} > {dstPath}");
        }
        Debug.LogWarning("完成热更打包");
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

    private static void CopyDirs(string sour, string dst, string exclude)
    {
        var files = Directory.GetFiles(sour);
        if (!Directory.Exists(dst)) Directory.CreateDirectory(dst);
        foreach (var file in files)
        {
            var fn = Path.GetFileName(file);
            if (fn.EndsWith(exclude)) continue;
            File.Copy(file, $"{dst}/{fn}", true);
        }
        var dirs = Directory.GetDirectories(sour);
        foreach (var dir in dirs)
        {
            var dn = Path.GetFileName(dir);
            CopyDirs(dir, $"{dst}/{dn}", exclude);
        }
    }

    // 从构建命令里获取参数示例
    private static Dictionary<string,string> GetCommandLineArgs()
    {
        var dic=new Dictionary<string,string>();
        foreach (string arg in System.Environment.GetCommandLineArgs())
        {
            var kv = arg.Split('=');
            if (kv.Length == 2) {
                dic[kv[0]] = kv[1];
            }
        }
        return dic;
    }

}
