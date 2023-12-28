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
using UnityEditor.VersionControl;
#if UNITY_WX
using WeChatWASM;
using static WeChatWASM.WXConvertCore;
#endif

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
        PrebuildCommand.GenerateAll();
        FullBuild_WithoutHyCLRGenerateAll();
    }
    [MenuItem("XIHUtil/Jenkins/FullBuild_WithoutHyCLRGenerateAll")]
    public static void FullBuild_WithoutHyCLRGenerateAll() {
        HotBuild();

        var curTarget = EditorUserBuildSettings.activeBuildTarget;
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
#if UNITY_WX
                    WXSettings();
                    if (WXExportError.SUCCEED == WXConvertCore.DoExport())
                    {
                        //为了更好支持分包，默认把 XIHWebServerRes\MiniGame\webgl/**.data.unityweb.bin.txt文件拷贝到 XIHWebServerRes/WebGL 下
                        var config = UnityUtil.GetEditorConf();
                        var webglPath = config.ProjectConf.DST + "/webgl";
                        var files = Directory.GetFiles(webglPath, "*.webgl.data.unityweb.bin.txt");
                        var dstRoot = $"{WEB_ROOT}/WebGL/";
                        foreach (var file in files)
                        {
                            var dst = dstRoot + Path.GetFileName(file);
                            Debug.LogWarning($"分包 > {file} > {dst}");
                            File.Copy(file, dst, true);
                        }
                    }
                    else { 
                        Debug.LogError("转换小游戏失败");
                    }
                    return;
#else
                    buildOptions = BuildOptions.CompressWithLz4HC;
                    targetPath = $"{WEB_ROOT}/GameWebGL/";
                    if (Directory.Exists(targetPath)) Directory.Delete(targetPath, true);
                    Directory.CreateDirectory(targetPath);
                    break;
#endif
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
        Debug.LogWarning("YooAsset开始构建");
        ExecuteYooAssetBuild(curTarget);
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
        var buildinFileCopyOption = EBuildinFileCopyOption.None;
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
        if (buildResult.Success) {
            Debug.LogWarning("打包资源AB成功");
            var dstPath = $"{WEB_ROOT}/{buildTarget}";
            if (Directory.Exists(dstPath)) Directory.Delete(dstPath, true);
            var srcPath = buildResult.OutputPackageDirectory;
            CopyDirs(srcPath, dstPath, new HashSet<string>() { ".json" ,".xml"});
            Debug.LogWarning($"拷贝到目标目录:{srcPath} > {dstPath}");
        }
        else
        {
            Debug.LogError($"YooAsset 构建失败 : {buildResult.FailedTask}");
        }
        return buildResult.Success;
    }

    private static string GetDefaultPackageVersion()
    {
        int totalMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
        return DateTime.Now.ToString("yyyy-MM-dd") + "-" + totalMinutes;
    }

    private static void CopyDirs(string sour, string dst, HashSet<string> exclude)
    {
        var files = Directory.GetFiles(sour);
        if (!Directory.Exists(dst)) Directory.CreateDirectory(dst);
        foreach (var file in files)
        {
            var fn = Path.GetFileName(file);
            var ext = Path.GetExtension(fn);
            if (exclude.Contains(ext)) continue;
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
#if UNITY_WX
    static void WXSettings() {
        var config = UnityUtil.GetEditorConf();

        //设置输出相对路径
        config.ProjectConf.DST = $"{WEB_ROOT}/MiniGame";
        Debug.LogWarning($"设置小游戏输出相对路径 {config.ProjectConf.DST}");

        var remoteCfgPath = $"{WEB_ROOT}/Front/WebGL.json";
        if (File.Exists(remoteCfgPath))
        {
            var fontCfg = JsonUtility.FromJson<FrontConfig>(File.ReadAllText(remoteCfgPath));
            config.ProjectConf.CDN = fontCfg.defaultHostServer;
            Debug.LogWarning($"为了方便分包才设置默认CDN，AOT2HOT后都是走代码设置的CDN，当前默认通过{remoteCfgPath}文件设置CDN：{fontCfg.defaultHostServer}\n 这里设置defaultHostServer而不是cdn是为了平台差异化，不然分包要放在{WEB_ROOT}，而不是{WEB_ROOT}/WebGL");
        }        
        else
        {
            Debug.LogError($"未找到{remoteCfgPath}文件,CDN保留原始设置");
        }

        //排除缓存为该后缀的文件
        var str = config.ProjectConf.bundleExcludeExtensions;
        var hash = new HashSet<string>(str.Split(";"));
        hash.Remove("");
        hash.Add("version");
        config.ProjectConf.bundleExcludeExtensions=string.Join(";", hash);

        //缓存www下载中包含WebGL路径的文件
        str = config.ProjectConf.bundlePathIdentifier;
        hash = new HashSet<string>(str.Split(";"));
        hash.Remove("");
        hash.Add("WebGL");
        config.ProjectConf.bundlePathIdentifier = string.Join(";", hash);

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssetIfDirty(config);
        AssetDatabase.Refresh();
    }
#endif
}
