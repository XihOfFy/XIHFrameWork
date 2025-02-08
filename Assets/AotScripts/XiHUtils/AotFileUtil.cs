#if UNITY_WX && !UNITY_EDITOR
#define UNITY_WX_WITHOUT_EDITOR
#endif
#if UNITY_WX_WITHOUT_EDITOR
using WeChatWASM;
#endif
using System.IO;
using System.Text;
using UnityEngine;


namespace Aot.XiHUtil
{
    public class AotFileUtil
    {
        public static readonly string RootPath;
        public const string SAVE_DIR = "SaveDir";
        public static readonly string SavePath;
        public const string SAVE_FRONT = "front.cfg";


#if UNITY_WX_WITHOUT_EDITOR
        static WXFileSystemManager fileSystemManager;
#elif UNITY_DY
        static TTSDK.TTFileSystemManager fileSystemManager;
#elif UNITY_KS
        static KSWASM.KSFileSystemManager fileSystemManager;
#elif UNITY_HW_QG
        static HWWASM.FileSystemManager fileSystemManager;
#endif
        static AotFileUtil()
        {
#if UNITY_EDITOR
            RootPath = Application.dataPath;
#elif UNITY_STANDALONE
            RootPath = Application.streamingAssetsPath;
#elif UNITY_WX
            RootPath = WX.env.USER_DATA_PATH;
#elif UNITY_KS
            RootPath = KSWASM.KS.USER_DATA_PATH;
#elif UNITY_HW_QG
            RootPath = HWWASM.QG.Env.UserDataPath;
//#elif UNITY_DY
//https://developer.open-douyin.com/docs/resource/zh-CN/mini-game/develop/guide/game-engine/rd-to-SCgame/c-api/file/file-system-manager
//目前 Native 方案下效果与 File.Exists || Directory.Exists 一致，路径参数不能使用 scfile 协议头，请使用 Application.persistentDataPath 等原生接口。
//            RootPath = tt.getEnvInfoSync.USER_DATA_PATH;
#else
            RootPath = Application.persistentDataPath;
#endif
            SavePath = $"{RootPath}/{SAVE_DIR}";
#if UNITY_EDITOR
            SavePath = SAVE_DIR;
#endif

#if UNITY_WX_WITHOUT_EDITOR
            fileSystemManager = WX.GetFileSystemManager();
            if (!DirExist(SavePath)) fileSystemManager.MkdirSync(SavePath, true);//js error dont need catch
#elif UNITY_HW_QG
            fileSystemManager = HWWASM.QG.GetFileSystemManager();
            if (!DirExist(SavePath)) fileSystemManager.MkdirSync(SavePath, true);//js error dont need catch
#elif UNITY_DY
            fileSystemManager = TTSDK.TT.GetFileSystemManager();
            if (!DirExist(SavePath)) fileSystemManager.MkdirSync(SavePath, true);
#elif UNITY_KS
            fileSystemManager = KS.GetFileSystemManager();
            if (!DirExist(SavePath)) fileSystemManager.MkdirSync(SavePath, true);//js error dont need catch
#else
            if (!DirExist(SavePath)) Directory.CreateDirectory(SavePath);
#endif
        }

        public static void WriteFile(string relativePath, string content)
        {
            var fullPath = SavePath + "/" + relativePath;
#if UNITY_WX_WITHOUT_EDITOR
            fileSystemManager.WriteFileSync(fullPath, content);
#elif UNITY_HW_QG
            HWWASM.QG.GetFileSystemManager().WriteFileSync(new HWWASM.WriteFileStringSyncOption()
            {
                filePath = fullPath,
                data = content
            });
#elif UNITY_DY
            fileSystemManager.WriteFileSync(fullPath, content);
#elif UNITY_KS
            Debug.Log($"写入,path{fullPath}：content:{content}");
            fileSystemManager.WriteFileSync(fullPath, content);
#else
            File.WriteAllText(fullPath, content, UTF8Encoding.UTF8);
#endif
        }
        public static void WriteFile(string relativePath, byte[] content)
        {
            var fullPath = SavePath + "/" + relativePath;
#if UNITY_WX_WITHOUT_EDITOR
            fileSystemManager.WriteFileSync(fullPath, content);
#elif UNITY_HW_QG
            HWWASM.QG.GetFileSystemManager().WriteFileSync(new HWWASM.WriteFileBinarySyncOption()
            {
                filePath = fullPath,
                data = content
            });
#elif UNITY_DY
            fileSystemManager.WriteFileSync(fullPath, content);
#elif UNITY_KS
            fileSystemManager.WriteFileSync(fullPath, content);
#else
            File.WriteAllBytes(fullPath, content);
#endif
        }
        public static string ReadFile(string relativePath)
        {
            var fullPath = SavePath + "/" + relativePath;
            if (!FileExist(fullPath)) return "";
#if UNITY_WX_WITHOUT_EDITOR
            return fileSystemManager.ReadFileSync(fullPath,"utf8");
#elif UNITY_DY
            return fileSystemManager.ReadFileSync(fullPath, "utf8");
#elif UNITY_KS
            return fileSystemManager.ReadFileSync(fullPath, "utf8");
#elif UNITY_HW_QG
            var res = HWWASM.QG.GetFileSystemManager().ReadFileSync(new HWWASM.ReadFileStringSyncOption()
            {
                filePath = fullPath,
            });
            return res.data;
#else
            return File.ReadAllText(fullPath, UTF8Encoding.UTF8);
#endif
        }
        public static byte[] ReadFileBytes(string relativePath)
        {
            var fullPath = SavePath + "/" + relativePath;
            if (!FileExist(fullPath)) return null;
#if UNITY_WX_WITHOUT_EDITOR
            return fileSystemManager.ReadFileSync(fullPath);
#elif UNITY_DY
            return fileSystemManager.ReadFileSync(fullPath);
#elif UNITY_KS
            return fileSystemManager.ReadFileSync(fullPath);
#elif UNITY_HW_QG
            var res = HWWASM.QG.GetFileSystemManager().ReadFileSync(new HWWASM.ReadFileBinarySyncOption()
            {
                filePath = fullPath,
            });
            return res.data;
#else
            return File.ReadAllBytes(fullPath);
#endif
        }
        public static void DeleteFile(string relativePath)
        {
            var fullPath = SavePath + "/" + relativePath;
            if (!FileExist(fullPath)) return;
#if UNITY_WX_WITHOUT_EDITOR
            fileSystemManager.UnlinkSync(fullPath);
#elif UNITY_DY
            fileSystemManager.UnlinkSync(fullPath);
#elif UNITY_KS
            fileSystemManager.UnlinkSync(fullPath);
#elif UNITY_HW_QG
            fileSystemManager.UnlinkSync(fullPath);
#else
            File.Delete(fullPath);
#endif
        }
        static bool FileExist(string fullPath)
        {
#if UNITY_WX_WITHOUT_EDITOR
            var exist = "access:ok".Equals(fileSystemManager.AccessSync(fullPath));
#elif UNITY_DY
            var exist = fileSystemManager.AccessSync(fullPath);
#elif UNITY_KS
            var exist = "access:ok".Equals(fileSystemManager.AccessSync(fullPath));
#elif UNITY_HW_QG
            var res = HWWASM.QG.GetFileSystemManager().AccessSync(fullPath);
            var exist = res.isSuccess;
#else
            var exist = File.Exists(fullPath);
#endif
            return exist;
        }
        static bool DirExist(string fullPath)
        {
#if UNITY_WX_WITHOUT_EDITOR
            var exist = "access:ok".Equals(fileSystemManager.AccessSync(fullPath));
            return exist;
#elif UNITY_DY
            var exist = fileSystemManager.AccessSync(fullPath);
            return exist;
#else
            return Directory.Exists(fullPath);
#endif
        }
    }
}
