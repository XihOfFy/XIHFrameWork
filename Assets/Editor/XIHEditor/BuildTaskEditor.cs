using System;
using System.IO;
using System.Text;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
#if UNITY_WEBGL && USE_ZSSDK
public class WebGLEmscriptenBuildFixer : IPreprocessBuildWithReport
{
    public int callbackOrder => 1;
    public void OnPreprocessBuild(BuildReport report)
    {
        Environment.SetEnvironmentVariable("PYTHONUTF8", "1");
    }
}
#endif
public class LocalizationBuildPlayerAndroid : IPostGenerateGradleAndroidProject,IPostprocessBuildWithReport
{
    public int callbackOrder { get; }
    public void OnPostGenerateGradleAndroidProject(string path)
    {
        var src = "Assets/Plugins/Android/res~";
        var dst = path + "/src/main/res";
        Debug.Log($"OnPostGenerateGradleAndroidProject Copy {src} >>  {dst}");
        CopyFolder(src, dst);

        src = "Assets/Plugins/Android/assets~";
        dst = path + "/src/main/assets";
        Debug.Log($"OnPostGenerateGradleAndroidProject Copy {src} >>  {dst}");
        CopyFolder(src, dst);

    }
    public void OnPostprocessBuild(BuildReport report)
    {
		if (report.summary.platform != UnityEditor.BuildTarget.Android) return;
        var path = report.summary.outputPath;
#if USE_ZSSDK
        var dst = path + "/gradle/wrapper/gradle-wrapper.properties";
        var lines = File.ReadAllLines(dst);
        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            if (line.TrimStart().StartsWith("distributionUrl="))
            {
                //sb.AppendLine("distributionUrl=https\\://services.gradle.org/distributions/gradle-7.6-bin.zip");
                //sb.AppendLine("distributionUrl=https\\://services.gradle.org/distributions/gradle-8.13-bin.zip");
                sb.AppendLine("distributionUrl=https\\://mirrors.cloud.tencent.com/gradle/gradle-8.13-bin.zip");
            }
            else
            {
                sb.AppendLine(line);
            }
        }
        File.WriteAllText(dst, sb.ToString());
#endif
    }

    public static void CopyFolder(string from, string to)
    {
        if (!Directory.Exists(from)) return;
        if (!Directory.Exists(to))
            Directory.CreateDirectory(to);

        // 子文件夹
        foreach (string sub in Directory.GetDirectories(from))
            CopyFolder(sub, to + "/" + Path.GetFileName(sub));

        // 文件
        foreach (string file in Directory.GetFiles(from))
        {
            var dst = to + "/" + Path.GetFileName(file);
            if (file.EndsWith(".meta") /*|| File.Exists(dst)*/) continue;
            File.Copy(file, dst, true);
        }
    }
}
