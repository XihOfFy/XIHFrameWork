#if UNITY_WX && !UNITY_EDITOR
#define UNITY_WX_WITHOUT_EDITOR
#endif
#if UNITY_WX
using WeChatWASM;
#endif
using System.IO;
using System.Text;
using UnityEngine;


namespace Aot
{
    public class AotFileUtil 
    {
        static readonly string RootPath;
        public const string SAVE_FRONT = "front.cfg";
        static AotFileUtil()
        {
#if UNITY_EDITOR
            RootPath = "SaveDir";
#elif UNITY_STANDALONE
            RootPath = Application.streamingAssetsPath;
#elif UNITY_WX
            RootPath = WX.env.USER_DATA_PATH;
//#elif UNITY_DY
//            RootPath = StarkSDKSpace.StarkFileSystemManager.USER_DATA_PATH;
#else
            RootPath = Application.persistentDataPath;
#endif
        }

        public static void WriteFile(string relativePath, string content)
        {
            var fullPath = RootPath + "/" + relativePath;
#if UNITY_WX_WITHOUT_EDITOR
            WX.GetFileSystemManager().WriteFileSync(fullPath, content);
#elif UNITY_DY
            StarkSDKSpace.StarkSDK.API.GetStarkFileSystemManager().WriteFileSync(fullPath, content);
#else
            File.WriteAllText(fullPath, content, UTF8Encoding.UTF8);
#endif
        }
        public static string ReadFile(string relativePath)
        {
            var fullPath = RootPath + "/" + relativePath;
#if UNITY_WX_WITHOUT_EDITOR
            var exist = "access:ok".Equals(WX.GetFileSystemManager().AccessSync(fullPath));
            if (!exist) return "";
            return WX.GetFileSystemManager().ReadFileSync(fullPath,"utf8");
#elif UNITY_DY
            var exist = StarkSDKSpace.StarkSDK.API.GetStarkFileSystemManager().AccessSync(fullPath);
            if (!exist) return "";
            return StarkSDKSpace.StarkSDK.API.GetStarkFileSystemManager().ReadFileSync(fullPath, "utf8");
#else
            if (!File.Exists(fullPath)) return "";
            return File.ReadAllText(fullPath, UTF8Encoding.UTF8);
#endif
        }
    }
}
