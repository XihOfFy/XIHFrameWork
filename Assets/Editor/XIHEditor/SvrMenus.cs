using System.IO;
using UnityEditor;

namespace XIHBasic {
    public sealed class SvrMenus
    {
        [MenuItem("XIHUtil/Server/WebSvr")]
        static void StartWebSvr()
        {
            ProcessUtil.Run("dotnet", "../../Res/WebBin/net5.0/XIHEmptyWeb.dll", "./XIHServer/Tools/XIHEmptyWeb");
        }
        [MenuItem("XIHUtil/Server/GameSvr")]
        static void StartGameSvr()
        {
            ProcessUtil.Run("dotnet", "XIHServer.dll", "./XIHServer/Res/ServerBin/net5.0");
        }
        const string LINUX_IP = "fy@192.168.25.128";
        [MenuItem("XIHUtil/Server/RSyncSvr")]
        static void StartRSync()
        {
            ProcessUtil.Run(Path.GetFullPath("./XIHServer/Tools/cwrsync/rsync.exe"), $"-vzrtopg --password-file={Path.GetFullPath("./XIHServer/Tools/cwrsync/config/rsync.secrets")} --exclude-from={Path.GetFullPath("./XIHServer/Tools/cwrsync/config/exclude.txt")} --delete ./ {LINUX_IP}::Sync/ --chmod=ugo=rwX", "./XIHServer/Res");
            //ProcessUtil.Run("./rsync.exe", $"-vzrtopg --password-file=./config/rsync.secrets --exclude-from=./config/exclude.txt --delete ./ {LINUX_IP}::Sync/ --chmod=ugo=rwX", "./XIHServer/Tools/cwrsync");
        }
    }
}
