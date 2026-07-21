#define REDIRECT
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Tmpl
{
    public class TmplWindow : EditorWindow
    {

        [InitializeOnLoadMethod]
        static void InitTmpl() {
            Tables.InitFromEditor();
        }

        [MenuItem("GameUtil/Tmpl")]
        static void OpenWindow()
        {
            EditorWindow.GetWindow<TmplWindow>(true, "Tmpl Util");
        }
        public const string BAT_PATH= "LubanTmpl/gen.bat";

        private void OnGUI()
        {
            if (GUILayout.Button("输出Bin"))
            {
                Cmd(BAT_PATH);
                Tables.InitFromEditor();
                Debug.Log($"输出Bin完成");
            }
        }
        public static void Cmd(string cmdPath)
        {
            if (!File.Exists(cmdPath))
            {
                Debug.LogWarning($"请选择Tmpl的工作目录,该目录应该包含{cmdPath}文件");
                return;
            }
            cmdPath = Path.GetFullPath(cmdPath);
            //ProcessStart(cmdPath, Path.GetDirectoryName(cmdPath), "AutoClose");
            ProcessStart(cmdPath, Path.GetDirectoryName(cmdPath), "");
            AssetDatabase.Refresh();
        }

        static void ProcessStart(string cmdPath,string workDir,string arguments) {
            var start = new ProcessStartInfo
            {
                FileName = cmdPath,
                WorkingDirectory = workDir,
#if REDIRECT
                UseShellExecute = false,
                Arguments=arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = false,//这个不建议设置为true
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                CreateNoWindow=true,
#endif
            };
            using (Process process = Process.Start(start))
            {
#if REDIRECT
                var output = process.StandardOutput.ReadToEnd();
                if (!string.IsNullOrEmpty(output)) Debug.Log(output);
                output = process.StandardError.ReadToEnd();
                if(!string.IsNullOrEmpty(output)) Debug.LogError(output);
#endif
                process.WaitForExit();
            }
        }
        internal static string FullPath2RelativePath(string fullPath)
        {
            fullPath = Path.GetFullPath(fullPath);
            var curPath = Path.GetFullPath(".").Replace("\\", "/");
            fullPath = fullPath.Replace("\\", "/");
            var relativePath = fullPath.Substring(curPath.Length);
            if (relativePath.StartsWith("/")) relativePath = relativePath.TrimStart('/');
#if UNITY_EDITOR_WIN
            relativePath = "..\\" + relativePath.Replace("/","\\");
#else
            relativePath = "../" + relativePath;
#endif
            Debug.LogWarning($"fullPath={fullPath}\ncurPath={curPath}\nrelativePath={relativePath}");
            return relativePath;
        }
    }
}