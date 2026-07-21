using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using YooAsset.Editor;
using Aot;
using System;
using System.IO;
using Tmpl;
using System.Reflection;
using YooAsset.Editor;
using YooAsset;
using Aot2Hot;
using System.Text;
using System.Linq;
using UnityEditor.WebGL;
using Aot.XiHUtil;
using Obfuz4HybridCLR;
#if UNITY_WX
using WeChatWASM;
using static WeChatWASM.WXConvertCore;
#endif
#if (UNITY_DY || UNITY_TT)
using TTSDK.Tool;
#endif

//需要自行保证YooAsset资源收集规则已经提前设置了
public class JenkinsSupport
{
    //命令行参考: https://docs.unity.cn/cn/2021.1/Manual/CommandLineArguments.html
    //Jenkins安装了Unity插件的话，可以在Editor command line arguments 填写：
    //-quit -nographics -batchmode -buildTarget "${BuildTarget}" -projectPath "${ProjectRoot}" -executeMethod JenkinsSupport.JenkinsBuild buildID="${BUILD_ID}" fullBuild=${UseFullBuild} -logFile "${ProjectRoot}/JenkinsUnityBuildLog.log"

    const string WEB_ROOT = "XIHWebServerRes";
    [MenuItem("XIHUtil/Jenkins/PrintPreLoadFileList")]
    public static void PrintPreLoadFileList()
    {
#if UNITY_WEBGL
        var files = GetPreLoadList();
        Debug.Log($"将以下填写到TT打包后的game.js的preloadDataList参数内\n{string.Join(",\n", files.Select(v => $"'{v}'"))}");
#else
        Debug.Log($"仅在小游戏支持预下载");
#endif
    }
    public static void JenkinsBuild()
    {
        var dic = GetCommandLineArgs();
        if (dic.ContainsKey("fullBuild"))
        {
            var val = dic["fullBuild"];
            bool.TryParse(val, out var result);
            if (result)
            {
                FullBuild();
            }
            else
            {
                HotBuild();
            }
        }
    }

    [MenuItem("XIHUtil/Jenkins/FullBuild")]
    public static void FullBuild()
    {
        //因为 HybridCLR 内会改变exportAsGoogleAndroidProject 值，如果遇到打包失败，则无法还原
        var curTarget = EditorUserBuildSettings.activeBuildTarget;
        if (curTarget == BuildTarget.Android)
        {
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        }
        if (Obfuz.Settings.ObfuzSettings.Instance.buildPipelineSettings.enable)
        {
            PrebuildCommandExt.GenerateAll();
        }
        else
        {
            PrebuildCommand.GenerateAll();
        }
        FullBuild_WithoutHyCLRGenerateAll();
    }
    [MenuItem("XIHUtil/Jenkins/FullBuild_WithoutHyCLRGenerateAll")]
    public static void FullBuild_WithoutHyCLRGenerateAll()
    {
        var curTarget = EditorUserBuildSettings.activeBuildTarget;
        HotBuild();//还是觉得有必要放打包前，方便打包时可以插入一些资源到包体内;但是微信需要分包时，打包后也要拷贝分包
        if (curTarget == BuildTarget.WebGL)
        {
            //#if !(UNITY_DY || UNITY_TT)
            Debug.LogWarning("设置webgl的图片压缩格式为ASTC，且代码为Size优化");
            EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.ASTC;
            UserBuildSettings.codeOptimization = CodeOptimization.Size;
            //#endif
        }
#if UNITY_WX
        WXSettings();
        if (WXExportError.SUCCEED == WXConvertCore.DoExport())
        {
            WXSubpackage(curTarget);//微信需要分包时，打包后也要拷贝分包
            SetWXPreloadFiles();
        }
        else
        {
            Debug.LogError("转换小游戏失败");
            return;
        }
#elif (UNITY_DY || UNITY_TT)
        //Native方案可以调用 StarkSDKTool.API.BuildMananger.Build，WebGL方案可以调用StarkSDKTool.Builder.BuildWebGL
#if UNITY_ANDROID
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[]{ UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });
        StarkBuilderSettings.Instance.framework = Framework.Native;
        StarkBuilderSettings.Instance.isWebGL2 = true;
        StarkBuilderSettings.Instance.urlCacheList = new string[] { "app02.yundooo.com" };
        StarkBuilderSettings.Instance.dontCacheFileNames = new string[] { "json", "version" };

        var outDir = $"{WEB_ROOT}/DYOutput";
        if(Directory.Exists(outDir))Directory.Delete(outDir,true);
        EditorPrefs.SetString("pref_sc_output_dir", outDir);//设置抖音输出目录 pref_sc_build_output_dir

        StarkBuilderSettings.Instance.apkOutputDir = $"{WEB_ROOT}/DYOutput";

        //var result = BuildDY(false, false, "screwlike", appId: "tta0eed6533fdea14307");
        //Debug.Log($"Native Build Start reuslt={result.Sucess},isCancelBuild={result.IsCancelBuild},OutputPath={result.OutputPath}");

        var res = StarkSDKTool.API.BuildManager.Build(Framework.Native,false);
        Debug.Log($"Native Build Start reuslt={res}");
#elif UNITY_WEBGL
        DYSettings();
#if UNITY_TT
        var setting = StarkBuilderSettings.Instance;
        var result = TTSDK.Tool.Builder.BuildWebGL(setting, setting.OutputDir,out var isCancelBuild);
        Debug.Log($"WebGL Build Start reuslt={result},isCancelBuild={isCancelBuild}");
        WritePreLoadList2TTGameJs();
#else
        //旧版直接打包zip，所以无法追加写入preloadDataList参数，只能手动写入，且seeg也会导致出问题
        //var result = TTSDK.Tool.API.BuildManager.Build(Framework.Wasm, false);
        //Debug.Log($"WebGL Build Start reuslt={result.Result},isCancelBuild={result.IsFaulted}");
        var setting = StarkBuilderSettings.Instance;//        var starkBuilderSettings = StarkBuilderSettings.LoadSettings();
        var result = TTSDK.Tool.Builder.BuildWebGL(setting, setting.OutputDir, out var isCancelBuild);
        Debug.Log($"WebGL Build Start reuslt={result},isCancelBuild={isCancelBuild}");
        WritePreLoadList2TTGameJs();
#endif
#endif
#else
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
                    if (!EditorUserBuildSettings.exportAsGoogleAndroidProject)
                    {
                        if (EditorUserBuildSettings.buildAppBundle) targetPath += $"Game{PlayerSettings.bundleVersion}.aab";
                        else targetPath += $"Game{PlayerSettings.bundleVersion}.apk";
                    }
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
                }
                break;
        }
        var report = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, targetPath, curTarget, buildOptions);
        Debug.Log($"构建结果: {report.summary.result}");
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Failed)
        {
            //若是无头模式构建，添加这个行
            //Environment.Exit(-1);
        }
#endif
    }
    [MenuItem("XIHUtil/Jenkins/HotBuild")]
    public static void HotBuild()
    {
        var curTarget = EditorUserBuildSettings.activeBuildTarget;

        Debug.LogError("发布FGUI,只有专业版支持命令行。所以先自己先提前发布吧");
        //BuildFairyGUI();
        Debug.LogWarning("开始热更构建");
        if (Obfuz.Settings.ObfuzSettings.Instance.buildPipelineSettings.enable)
        {
            PrebuildCommandExt.CompileAndObfuscateDll();
        }
        else {
            CompileDllCommand.CompileDll(curTarget);//使用混淆的CompileAndObfuscateHotUpdateAssemblies
        }
        Debug.LogWarning("拷贝热更Dll");
        DllCopyEditor.CopyDlls(curTarget);
        AssetDatabase.Refresh();
        Debug.LogWarning("YooAsset开始构建");
        if (ExecuteYooAssetBuild(curTarget))
        {
#if UNITY_WX
            WXSubpackage(curTarget);
#endif
            Debug.LogWarning("完成热更打包 成功");
        }
        else
        {
            Debug.LogWarning("完成热更打包 失败");
        }
    }

    //只有专业版支持，算了。自己先提前发布吧
    static void BuildFairyGUI()
    {
        var exePath = "D:\\Devkit\\FairyGUI-Editor\\FairyGUI-Editor.exe";
        if (!File.Exists(exePath))
        {
            Debug.LogError("未找到FairyGUI-Editor安装目录，需要自行先发布");
            return;
        }
        //FairyGUI-Editor -batchmode -p project_desc_file [-b package_names] [-t branch_name] [-o output_path] [-logFile log_file_path] https://fairygui.com/docs/editor/publish
        ProcessUtil.Run(exePath, "-batchmode -p ./BomsortFairyUI/FGUIProject.fairy -logFile ./fgui.txt", "", false);
    }
    static bool ExecuteYooAssetBuild(BuildTarget buildTarget)
    {
#if USE_YOO
        Debug.Log($"开始构建 : {buildTarget}");

        var buildoutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
        var streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();

        if (Directory.Exists(buildoutputRoot))
        {
            Debug.LogWarning($"清空AB构建目录:{buildoutputRoot}");
            Directory.Delete(buildoutputRoot, true);
        }

#if UNITY_WEBGL
        var buildinFileCopyOption = EBuildinFileCopyOption.None;
#else
        var buildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyAll;
#endif

        // 构建参数
        var buildParameters = new ScriptableBuildParameters();
        buildParameters.BuildOutputRoot = buildoutputRoot;
        buildParameters.BuildinFileRoot = streamingAssetsRoot;
        buildParameters.BuildPipeline = EBuildPipeline.ScriptableBuildPipeline.ToString();
        buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle; //必须指定资源包类型
        buildParameters.BuildTarget = buildTarget;
        buildParameters.PackageName = AotConfig.PACKAGE_NAME;
        buildParameters.PackageVersion = GetDefaultPackageVersion();
        buildParameters.VerifyBuildingResult = true;
        buildParameters.EnableSharePackRule = false; //启用共享资源构建模式，兼容1.5x版本
        buildParameters.FileNameStyle = EFileNameStyle.BundleName_HashName;
        buildParameters.BuildinFileCopyOption = buildinFileCopyOption;
        buildParameters.BuildinFileCopyParams = string.Empty;

#if UNITY_WEBGL
        buildParameters.EncryptionServices = null;
        //buildParameters.EncryptionServices = new EncryptionNone();
#else
        buildParameters.EncryptionServices = new AotMgr.EncryptionServices();
#endif

        buildParameters.CompressOption = ECompressOption.LZ4;
        buildParameters.ClearBuildCacheFiles = false; //不清理构建缓存，启用增量构建，可以提高打包速度！
        buildParameters.UseAssetDependencyDB = true; //使用资源依赖关系数据库，可以提高打包速度！

        // 执行构建
        var pipeline = new ScriptableBuildPipeline();
        var buildResult = pipeline.Run(buildParameters, true);
        if (buildResult.Success)
        {
            var dstPath = $"{WEB_ROOT}/{buildTarget}";
            if (Directory.Exists(dstPath)) Directory.Delete(dstPath, true);
            var srcPath = buildResult.OutputPackageDirectory;
            CopyDirs(srcPath, dstPath, new HashSet<string>() { ".json", ".xml", ".report" });
            if (!Directory.Exists("Assets/YooAssetGenerate")) Directory.CreateDirectory("Assets/YooAssetGenerate");
            File.Copy($"{srcPath}/link.xml", "Assets/YooAssetGenerate/link.xml", true);
            //删除多余catJson
            var catJson = $"Assets/StreamingAssets/yoo/{AotConfig.PACKAGE_NAME}/{DefaultBuildinFileSystemDefine.BuildinCatalogJsonFileName}";
            if (File.Exists(catJson)) File.Delete(catJson);
            Debug.LogWarning($"构建成功,拷贝到目标目录:{srcPath} > {dstPath}");
        }
        else
        {
            Debug.LogError($"构建失败 : {buildResult.ErrorInfo}");
        }
        return buildResult.Success;
#else
        return true;
#endif
    }
    private static string GetDefaultPackageVersion()
    {
        if (string.IsNullOrEmpty(JenkinsEntry.resVersion))
        {
            Debug.LogWarning("使用时间戳作为资源版本");
            int totalMinutes = DateTime.Now.Hour * 100 + DateTime.Now.Minute;
            return DateTime.Now.ToString("yyyy-MM-dd") + "-" + totalMinutes;
        }
        else
        {
            Debug.LogWarning("使用自定义版本作为资源版本");
            return JenkinsEntry.resVersion;
        }
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
    private static Dictionary<string, string> GetCommandLineArgs()
    {
        var dic = new Dictionary<string, string>();
        foreach (string arg in System.Environment.GetCommandLineArgs())
        {
            var kv = arg.Split('=');
            if (kv.Length == 2)
            {
                dic[kv[0]] = kv[1];
            }
        }
        return dic;
    }
#if UNITY_WX
    static void WXSettings()
    {
        var config = WeChatWASM.UnityUtil.GetEditorConf();
        var outDir = $"{WEB_ROOT}/MiniGame";
        if (Directory.Exists(outDir)) Directory.Delete(outDir, true);
        //设置输出相对路径
        config.ProjectConf.relativeDST = outDir;
        config.ProjectConf.DST = Path.GetFullPath(outDir);
        Debug.LogWarning($"设置小游戏输出相对路径 {config.ProjectConf.DST}");

        config.CompileOptions.Il2CppOptimizeSize = true;//压缩包体大小
        var remoteCfgPath = $"{WEB_ROOT}/Front/WebGL.json";
        FrontConfig fontCfg = null;
        if (File.Exists(remoteCfgPath))
        {
            fontCfg = JsonUtility.FromJson<FrontConfig>(File.ReadAllText(remoteCfgPath));
            config.ProjectConf.CDN = fontCfg.cdn;
            Debug.LogWarning($"为了方便分包才设置默认CDN，AOT2HOT后都是走代码设置的CDN，当前默认通过{remoteCfgPath}文件设置CDN：{fontCfg.cdn}\n 若资源分包记得分包要放在{WEB_ROOT}，而不是{WEB_ROOT}/WebGL");
        }
        else
        {
            Debug.LogError($"未找到{remoteCfgPath}文件,CDN保留原始设置");
        }
        var cdn = fontCfg.cdn;
        if (cdn.EndsWith('/')) cdn = cdn.TrimEnd('/');
        var bundlePathIdentifier = cdn.Substring(cdn.LastIndexOf('/') + 1);
        cdn = cdn.Substring(0, cdn.LastIndexOf('/'));//+ "/" + BuildTargetGroup.WebGL.ToString();
        config.ProjectConf.CDN = cdn;
        Debug.LogWarning($"为了方便分包才设置默认CDN，AOT2HOT后都是走代码设置的CDN，当前默认通过{nameof(XIHAppSetting)}文件设置CDN：{cdn}, 后面最后为固定缓存文件夹名字：{bundlePathIdentifier}");

        //使用包内加载，且不压缩首包
        config.ProjectConf.assetLoadType = 1;
        config.ProjectConf.compressDataPackage = false;

        //排除缓存为该后缀的文件
        var str = config.ProjectConf.bundleExcludeExtensions;
        var hash = new HashSet<string>(str.Split(";"));
        hash.Remove("");
        hash.Add("version");
        //hash.Add("cfg");
        //hash.Add("json");
        config.ProjectConf.bundleExcludeExtensions = string.Join(";", hash);

        //缓存www下载中包含WebGL路径的文件
        str = config.ProjectConf.bundlePathIdentifier;
        hash = new HashSet<string>(str.Split(";"));
        hash.Remove("");
        hash.Add(bundlePathIdentifier);
        //hash.Add("WebGL");
        config.ProjectConf.bundlePathIdentifier = string.Join(";", hash);
        config.CompileOptions.enableIOSPerformancePlus = true;
		config.ProjectConf.MemorySize = 128;

        /* var prefiles = GetPreloadFiles(cfg.resUrl);//不支持，wx是streamingasset加载和预判断，所以只能手动设置
         config.ProjectConf.preloadFiles = string.Join(';', prefiles);*/

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssetIfDirty(config);
        AssetDatabase.Refresh();
    }
    static void WXSubpackage(BuildTarget buildTarget)
    {
        //为了更好支持分包，默认把 XIHWebServerRes\MiniGame\webgl/**.data.unityweb.bin.txt文件拷贝到 XIHWebServerRes/WebGL 下
        var config = WeChatWASM.UnityUtil.GetEditorConf();
        if (config.ProjectConf.assetLoadType == 1)
        {
            Debug.LogWarning("当前首包资源加载方式方式为小游戏包内，不需要拷贝到WebGL");
            return;
        }
        var webglPath = config.ProjectConf.DST + "/webgl";
        if (!Directory.Exists(webglPath)) return;
        //config.ProjectConf.compressDataPackage = true;//使用压缩br,没用，除非提前知道
        var files = Directory.GetFiles(webglPath, "*.webgl.data.unityweb.bin.*");//不同压缩分包后缀不一样，txt；br
        var dstRoot = $"{WEB_ROOT}/{buildTarget}/";
        if (!Directory.Exists(dstRoot)) Directory.CreateDirectory(dstRoot);
        foreach (var file in files)
        {
            var dst = dstRoot + Path.GetFileName(file);
            Debug.LogWarning($"微信小游戏分包 > {file} > {dst}");
            File.Copy(file, dst, true);
        }
    }

#endif
    static void SetWXPreloadFiles()
    {
        var orders = GetPreLoadList();
        var addLine = string.Join(",\r\n", orders.Select(s => $"'{s}'"));
        var configJs = $"{WEB_ROOT}/MiniGame/minigame/game.js";
        var lines = File.ReadAllLines(configJs);
        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            sb.AppendLine(line);
            if (line.Trim().StartsWith("preloadDataList:"))
            {
                sb.AppendLine(addLine);
            }
        }
        File.WriteAllText(configJs, sb.ToString());
    }

    static List<string> GetPreLoadList()
    {
        var cfg = Resources.Load<XIHAppSetting>(nameof(XIHAppSetting));
        var resUrl = cfg.resUrl;
        if (resUrl.EndsWith('/')) resUrl = resUrl.TrimEnd('/');
        var dic = new Dictionary<string, int>();
        var initNeedAbPreffixs = new string[] {
            "assets_res_aot2hot_",
            "assets_res_tmpl_",
            "firstgame_",
            "firstgame2_",
            "poolobj_",
            "assets_res_pooldepend_",
            "assets_res_poolobj_",
            "assets_res_fairyres_common_",
            "assets_res_fairyres_scene_",
            "unityshaders_",
            "assets_res_hotscene_",
            "assets_res_game1_",
            "assets_res_prefab_map_1_",
            "assets_res_fairyres_home_",
            "assets_res_fairyres_game_",
            //"assets_res_audio_music_",
        };
        var suffixs = new string[] { ".bytes", ".hash" };
        var outDir = $"{WEB_ROOT}/{BuildTarget.WebGL}";
        var files = Directory.GetFiles(outDir);
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var idx = -1;
            var find = false;
            foreach (var preffix in initNeedAbPreffixs)
            {
                ++idx;
                if (fileName.StartsWith(preffix))
                {
                    dic[$"{resUrl}/{fileName}"] = idx;
                    find = true;
                    break;
                }
            }
            if (find) continue;
            foreach (var sfuffix in suffixs)
            {
                if (fileName.EndsWith(sfuffix))
                {
                    dic[$"{resUrl}/{fileName}"] = -1;
                    break;
                }
            }
        }

        var orders = dic.OrderBy(kv => kv.Value).Select(kv => kv.Key);
        return orders.ToList();
    }

#if (UNITY_DY || UNITY_TT)
    //因为TT的PreLoadList无法成功写入，这里手动写入下
    static void WritePreLoadList2TTGameJs()
    {
        var outDir = $"{WEB_ROOT}/DYWebGL" + "/tt-minigame/game.js";
        //var starkBuilderSettings = StarkBuilderSettings.Instance;
        //var outDir = starkBuilderSettings.OutputDir + "/tt-minigame/game.js";
        if (File.Exists(outDir))
        {
            var orders = GetPreLoadList();
            var alltxt = File.ReadAllText(outDir);
            alltxt = alltxt.Replace("// '/WebGL/bundles_e1af572c458eda6944e73db25cae88d5'", string.Join(",\n", orders.Select(v => $"'{v}'")));
            File.WriteAllText(outDir, alltxt);
            Debug.LogWarning($"写入PreLoadList成功：outDir ={outDir}");
        }
        else
        {
            Debug.LogError($"写入PreLoadList失败，文件不存在：outDir ={outDir}");
        }
    }
    static void DYSettings()
    {
        var starkBuilderSettings = StarkBuilderSettings.Instance;//        var starkBuilderSettings = StarkBuilderSettings.LoadSettings();
        starkBuilderSettings.framework = Framework.Wasm;

        var cfg = Resources.Load<XIHAppSetting>(nameof(XIHAppSetting));
        var cdn = cfg.resUrl;
        if (cdn.EndsWith('/')) cdn = cdn.TrimEnd('/');
        var bundlePathIdentifier = cdn.Substring(cdn.LastIndexOf('/') + 1);
        cdn = cdn.Substring(0, cdn.LastIndexOf('/'));//+ "/" + BuildTargetGroup.WebGL.ToString();
        starkBuilderSettings.CDN = cdn;

        var outDir = $"{WEB_ROOT}/DYWebGL";
        if (Directory.Exists(outDir)) Directory.Delete(outDir, true);
        Directory.CreateDirectory(outDir);
        starkBuilderSettings.publishType = PublishType.AndroidWebGLWithIOS;
        starkBuilderSettings.webglPackagePath = outDir;
        starkBuilderSettings.OutputDir = outDir;
        starkBuilderSettings.useByteAudioAPI = false;
        starkBuilderSettings.wasmMemorySize = 128;
        starkBuilderSettings.isWebGL2 = true;//为了支持fanshe shader
        starkBuilderSettings.needCompress = true;
        starkBuilderSettings.buildOptions = BuildOptions.None;
        starkBuilderSettings.urlCacheList = new string[] { cdn };
        starkBuilderSettings.dontCacheFileNames = new string[] { "version" };
        starkBuilderSettings.iOSPerformancePlus = true;
#if UNITY_DY
        //新版抖音打包，输出和TT打包一致，使用新格式，输出tt-minigame文件夹而不是zip包，方便插入自定义修改
        starkBuilderSettings.isOldBuildFormat = false;
        starkBuilderSettings.dataLoadType = DataLoadType.Package;
#endif

        var orders = GetPreLoadList();
        var addLine = string.Join(";", orders);
        starkBuilderSettings.preloadFiles = addLine;

        EditorUtility.SetDirty(starkBuilderSettings);
        AssetDatabase.SaveAssetIfDirty(starkBuilderSettings);
        AssetDatabase.Refresh();

#if UNITY_WEBGL
        Debug.Log($"将以下填写到TT打包后的game.js的preloadDataList参数内\n{string.Join(",\n", orders.Select(v => $"'{v}'"))}");
#else
        Debug.Log($"仅在小游戏支持预下载");
#endif
    }
#endif
}
