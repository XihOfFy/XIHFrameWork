namespace XIHBasic {
    using UnityEditor;
    using UnityEngine;

    [System.Reflection.Obfuscation(Exclude = true)]
    [InitializeOnLoad]
    public class ILRuntimeMenu
    {
        [MenuItem("ILRuntime/打开ILRuntime中文文档")]
        static void OpenDocumentation()
        {
            Application.OpenURL("https://ourpalm.github.io/ILRuntime/");
        }

        [MenuItem("ILRuntime/打开ILRuntime Github项目")]
        static void OpenGithub()
        {
            Application.OpenURL("https://github.com/Ourpalm/ILRuntime");
        }
    }
}