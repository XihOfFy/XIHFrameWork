using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class SvrMenus
{
    [MenuItem("XIHUtil/Server/WebSvr")]
    static void StartWebSvr()
    {
        //TODO 拷贝AOT HOT DLL和ABs
        //ProcessUtil.Run("dotnet", "../../Res/WebBin/net5.0/XIHEmptyWeb.dll",Path.GetFullPath("./XIHServer/Tools/XIHEmptyWeb"));
        ProcessUtil.Run(Path.GetFullPath("./Packages/XIHWebServer/XIHEmptyWeb.exe"),"", "./Packages/XIHWebServer");
    }

    [MenuItem("XIHUtil/Run AOT Splash Scene")]
    static void StartRunXIHBaseEnter()
    {
        if (!EditorApplication.isPlaying)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene("Assets/Res/Scene/Aot2HotScene/Aot2Hot.unity");
            EditorApplication.isPlaying = !EditorApplication.isPlaying;
        }
    }
}
