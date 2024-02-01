#if UNITY_WX && !UNITY_EDITOR
#define UNITY_WX_WITHOUT_EDITOR
#endif
#if UNITY_WX
using WeChatWASM;
#endif
using System.IO;
using System.Text;
using UnityEngine;

namespace XiHUtil
{
    public static class FileUtil
    {
        public static readonly string RootPath;
        public const string SAVE_DIR = "SaveDir";
        public static readonly string SavePath;
        static FileUtil()
        {
#if UNITY_EDITOR
            RootPath = Application.dataPath;
#elif UNITY_STANDALONE
            RootPath = Application.streamingAssetsPath;
#elif UNITY_WX
            RootPath = WX.env.USER_DATA_PATH;
#else
            RootPath = Application.persistentDataPath;
#endif
            SavePath = $"{RootPath}/{SAVE_DIR}";
#if UNITY_EDITOR
            SavePath = SAVE_DIR;
#endif
#if UNITY_WX_WITHOUT_EDITOR
            WX.GetFileSystemManager().MkdirSync(SavePath, true);//js error dont need catch
#else
            if (!Directory.Exists(SavePath)) Directory.CreateDirectory(SavePath);
#endif
        }

        public static void WriteFile(string relativePath, string content)
        {
            var fullPath = SavePath + "/" + relativePath;
#if UNITY_WX_WITHOUT_EDITOR
            WX.GetFileSystemManager().WriteFileSync(fullPath, content);
#else
            File.WriteAllText(fullPath, content, UTF8Encoding.UTF8);
#endif
        }
        public static void WriteFile(string relativePath, byte[] content)
        {
            var fullPath = SavePath + "/" + relativePath;
#if UNITY_WX_WITHOUT_EDITOR
            WX.GetFileSystemManager().WriteFileSync(fullPath, content);
#else
            File.WriteAllBytes(fullPath, content);
#endif
        }
        public static string ReadFile(string relativePath)
        {
            var fullPath = SavePath + "/" + relativePath;
#if UNITY_WX_WITHOUT_EDITOR
            return WX.GetFileSystemManager().ReadFileSync(fullPath,"utf8");
#else
            if (!File.Exists(fullPath)) return "";
            return File.ReadAllText(fullPath, UTF8Encoding.UTF8);
#endif
        }
        public static byte[] ReadFileBytes(string relativePath)
        {
            var fullPath = SavePath + "/" + relativePath;
#if UNITY_WX_WITHOUT_EDITOR
            return WX.GetFileSystemManager().ReadFileSync(fullPath);
#else
            if (!File.Exists(fullPath)) return null;
            return File.ReadAllBytes(fullPath);
#endif
        }
        public static void DeleteFile(string relativePath) {
            var fullPath = SavePath + "/" + relativePath;
#if UNITY_WX_WITHOUT_EDITOR
            WX.GetFileSystemManager().UnlinkSync(fullPath);
#else
            if (File.Exists(fullPath)) File.Delete(fullPath);
#endif
        }
    }
}
