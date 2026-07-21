using Aot;
using Hot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tmpl;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public class JenkinsEntry
{
    public static string resVersion;//资源版本指定 自定
    public static void JenkinsPreBuild()
    {
        var curTarget = EditorUserBuildSettings.activeBuildTarget;
        var curGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var dic = GetCommandLineArgs();
        var targetAndGroup = dic["targetAndGroup"].Trim();
        if (!targetAndGroup.Equals(curTarget.ToString()) && !targetAndGroup.Equals(curGroup.ToString()))
        {
            Debug.LogError($"当前Unity项目的平台是:{curGroup}>{curTarget}，无法打包{targetAndGroup}，需要自行先切换平台再使用Jenkins打包");
            EditorApplication.Exit(-1);//非稳定退出
            Environment.Exit(-1);
            //EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, target);//不做平台切换，项目参数固定，避免错误
            return;
        }

        var lastVerID = dic["lastVerID"].Trim();
        var appid = dic["appid"].Trim();
        var language = dic["language"].Trim();
        var resUrl = dic["resUrl"].Trim();
        var useAbb = "";
        if (dic.TryGetValue("useAbb", out var value))
        {
            useAbb = value.Trim();
        }
        TargetPreSetting(curGroup, lastVerID, appid, language, resUrl, useAbb);

        PlayerSettings.GetScriptingDefineSymbolsForGroup(curGroup, out var oldDefines);
        var defines = dic["defines"].Trim().Split(";");
        var definesSet = new HashSet<string>(defines);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(curGroup, defines);
        AssetDatabase.SaveAssets();
        if (defines.Length != oldDefines.Length)
        {
            Debug.LogWarning($"宏数量不一致，将重启:oldDefines={string.Join(";", oldDefines)}>> defines={string.Join(";", defines)}");
        }
        else
        {
            foreach (var set in oldDefines) definesSet.Remove(set);
            if (definesSet.Count > 0)
            {
                Debug.LogWarning($"宏定义不一致，将重启:oldDefines={string.Join(";", oldDefines)}>> defines={string.Join(";", defines)}");
            }
        }
        AssetDatabase.Refresh();
        EditorApplication.Exit(0);//稳定退出，让Unity执行下一步
        Environment.Exit(0);
    }

    public static void JenkinsBuild()
    {
        TargetSetting();
        var dic = GetCommandLineArgs();
        resVersion = dic[nameof(resVersion)].Trim();
        var buildType = dic["buildType"].Trim();
        Debug.Log($"Unity: 开始执行构建 {buildType}");

        //设置 sdk ndk 地址 -- 测试
        var ndkPath = EditorPrefs.GetString("AndroidNdkRoot");
        var sdkPath = EditorPrefs.GetString("AndroidSdkRoot");
        var jdkPath = EditorPrefs.GetString("JdkPath");
        UnityEngine.Debug.Log($"pre ndkPath {ndkPath}  sdkPath {sdkPath} jdkPath {jdkPath}");

        if (dic.ContainsKey("androidNdkPath"))
        {
            ndkPath = dic["androidNdkPath"].Trim();
            EditorPrefs.SetString("AndroidNdkRoot", ndkPath);
        }
        else
        {
            EditorPrefs.SetString("AndroidNdkRoot", EditorApplication.applicationContentsPath + "/PlaybackEngines/AndroidPlayer/NDK");
        }
        if (dic.ContainsKey("androidSdkPath"))
        {
            sdkPath = dic["androidSdkPath"].Trim();
            EditorPrefs.SetString("AndroidSdkRoot", sdkPath);
        }
        else
        {
            EditorPrefs.SetString("AndroidSdkRoot", EditorApplication.applicationContentsPath + "/PlaybackEngines/AndroidPlayer/SDK");
        }
        if (dic.ContainsKey("JdkPath"))
        {
            jdkPath = dic["JdkPath"].Trim();
            EditorPrefs.SetString("JdkPath", jdkPath);
        }
        else
        {
            EditorPrefs.SetString("JdkPath", EditorApplication.applicationContentsPath + "/PlaybackEngines/AndroidPlayer/OpenJDK");
        }
        /*
        #if UNITY_2022_3_OR_NEWER
                if (dic.ContainsKey("androidNdkPath"))
                {
                    ndkPath = dic["androidNdkPath"].Trim();
                    EditorPrefs.SetString("AndroidNdkRoot", ndkPath);
                }
                if (dic.ContainsKey("androidSdkPath"))
                {
                    sdkPath = dic["androidSdkPath"].Trim();
                    EditorPrefs.SetString("AndroidSdkRoot", sdkPath);
                }
                if (dic.ContainsKey("JdkPath"))
                {
                    jdkPath = dic["JdkPath"].Trim();
                    EditorPrefs.SetString("JdkPath", jdkPath);
                }
        #else
                EditorPrefs.DeleteKey("AndroidNdkRoot");
                EditorPrefs.DeleteKey("AndroidSdkRoot");
                EditorPrefs.DeleteKey("JdkPath");
        #endif*/

        ndkPath = EditorPrefs.GetString("AndroidNdkRoot");
        sdkPath = EditorPrefs.GetString("AndroidSdkRoot");
        jdkPath = EditorPrefs.GetString("JdkPath");
        UnityEngine.Debug.Log($"after ndkPath {ndkPath}  sdkPath {sdkPath} jdkPath {jdkPath}");


        Debug.LogWarning("输出Tmpl");
        TmplWindow.Cmd(TmplWindow.BAT_PATH);
        HybridCLR.Editor.Commands.LinkGeneratorCommand.GenerateLinkXml();
        AssetDatabase.Refresh();

        if (buildType.Equals("NewBuild"))
        {
            JenkinsSupport.FullBuild();
        }
        else if (buildType.Equals("ReBuild"))
        {
            JenkinsSupport.FullBuild_WithoutHyCLRGenerateAll();
        }
        else
        {
            JenkinsSupport.HotBuild();
        }
    }
    private static void TargetPreSetting(BuildTargetGroup group, string lastVerID, string appid, string language, string resUrl, string useAbb)
    {
        var cfg = Resources.Load<XIHFrontSetting>(nameof(XIHFrontSetting));
        cfg.appId = int.Parse(appid);
        if (Enum.TryParse<SystemLanguage>(language, true, out var res))
        {
            cfg.language = res;
        }
        else
        {
            cfg.language = SystemLanguage.Unknown;
            Debug.LogError($"不支持的语言: language={language}");
        }
        cfg.front = resUrl.Trim();
        EditorUtility.SetDirty(cfg);


        PlayerSettings.bundleVersion = $"{PlayerSettings.bundleVersion}.{lastVerID}";
#if UNITY_2022_3_OR_NEWER && UNITY_KS
        PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.FromBuildTargetGroup(group),Il2CppCodeGeneration.OptimizeSpeed);//使用裁剪il2cpp方式打包！
#elif UNITY_2022_3_OR_NEWER && UNITY_IOS
        PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.FromBuildTargetGroup(group),Il2CppCodeGeneration.OptimizeSpeed);//使用裁剪il2cpp方式打包！
#else
#if UNITY_2022_3_OR_NEWER
        PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.FromBuildTargetGroup(group), Il2CppCodeGeneration.OptimizeSize);//使用裁剪il2cpp方式打包！
#else
        EditorUserBuildSettings.il2CppCodeGeneration = Il2CppCodeGeneration.OptimizeSize;//使用裁剪il2cpp方式打包！
#endif
#endif
        PlayerSettings.SetManagedStrippingLevel(group, ManagedStrippingLevel.High);//使用高度裁剪托管代码
        switch (group)
        {
            case BuildTargetGroup.WebGL:
#if !UNITY_WX
                PlayerSettings.WebGL.template = "APPLICATION:Default";
                //PROJECT:WXTemplate2020 
#endif
                break;
            case BuildTargetGroup.Android:
                PlayerSettings.Android.bundleVersionCode = int.Parse(lastVerID);
                bool.TryParse(useAbb, out var isAbb);
                EditorUserBuildSettings.buildAppBundle = isAbb;
                if (isAbb)
                {//使用aab，需要使用rebuild打包，因为newbuild会将exportAsGoogleAndroidProject = false;
                    EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
                    PlayerSettings.SetApplicationIdentifier(group, "com.yunduo.ws3d.gp");
                    EditorUserBuildSettings.exportAsGoogleAndroidProject = true;//仍需导出AS，但是可以As打包aab
                }
                else
                {
                    EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                }
                break;
            case BuildTargetGroup.iOS:
                //PlayerSettings.iOS.buildNumber = lastVerID;
                break;
            default:
                Debug.LogError("不支持该平台自动化打包");
                break;
        }
        Debug.LogWarning($"TargetPreSetting appVer={PlayerSettings.bundleVersion}");
        AssetDatabase.SaveAssets();
    }
    private static void TargetSetting()
    {
        var curGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        switch (curGroup)
        {
            case BuildTargetGroup.WebGL:
                break;
            case BuildTargetGroup.Android:
                //PlayerSettings.Android.bundleVersionCode = lastVerID;
                PlayerSettings.Android.keystoreName = "Packages/your.keystore";
                PlayerSettings.Android.keystorePass = "your";
                PlayerSettings.Android.keyaliasName = "your";
                PlayerSettings.Android.keyaliasPass = "your";
                EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Public;
                break;
            case BuildTargetGroup.iOS:
                break;
            default:
                Debug.LogError("不支持该平台自动化打包");
                break;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
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
}
