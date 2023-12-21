#if UNITY_WX && !UNITY_EDITOR
#define UNITY_WX_WITHOUT_EDITOR
#endif
#if UNITY_WX
using WeChatWASM;
#endif
using System.IO;
using System.Text;
using UnityEngine;

namespace Hot
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

        public static void WriteFile(string relativePath, string content) {
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
            WX.GetFileSystemManager().WriteSync(new WriteSyncOption()
            {
                data = content,
                fd = WX.GetFileSystemManager().OpenSync(new OpenSyncOption()
                {
                    filePath = fullPath,
                    flag = "w+"
                }),
                encoding = "utf8"
            });
#else
            File.WriteAllBytes(fullPath, content);
#endif
        }
        public static string ReadFile(string relativePath)
        {
            var fullPath = SavePath + "/" + relativePath;
#if UNITY_WX_WITHOUT_EDITOR
            var bys = WX.GetFileSystemManager().ReadFileSync(fullPath);
            return UTF8Encoding.UTF8.GetString(bys);
#else
            return File.ReadAllText(fullPath, UTF8Encoding.UTF8);
#endif
        }
        public static byte[] ReadFileBytes(string relativePath)
        {
            var fullPath = SavePath + "/" + relativePath;
#if UNITY_WX_WITHOUT_EDITOR
            return WX.GetFileSystemManager().ReadFileSync(fullPath);
#else
            return File.ReadAllBytes(fullPath);
#endif
        }
    }
}
