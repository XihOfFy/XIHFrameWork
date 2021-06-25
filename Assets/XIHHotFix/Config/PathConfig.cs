using System.IO;
using XIHBasic;

namespace XIHHotFix
{
    public static class  PathConfig
    {
        public static string AACatalogPath => $"{PlatformConfig.PersistentDataPath}/com.unity.addressables";
        public static string ConfigPath=> $"{PlatformConfig.PersistentDataPath}/{PlatformConfig.CONFIG_NAME}";
        public static string DllPath => $"{PlatformConfig.PersistentDataPath}/{PlatformConfig.HOTFIX_DLL_NAME}";
        public const string AA_Scene_Load = "Assets/Bundles/Scenes/LoadScene.unity";
        public const string AA_Scene_Login = "Assets/Bundles/Scenes/LoginScene.unity";
        public const string AA_Scene_Battle = "Assets/Bundles/Scenes/BattleScene.unity";
        public const string AA_Scene_Lobby = "Assets/Bundles/Scenes/LobbyScene.unity";
        public static void ClearAll() {
            if (File.Exists(ConfigPath))
            {
                File.Delete(ConfigPath);
            }
            if (File.Exists(DllPath))
            {
                File.Delete(DllPath);
            }
            string pdbPath = $"{DllPath}.pdb";
            if (File.Exists(pdbPath))
            {
                File.Delete(pdbPath);
            }
            if (Directory.Exists(PathConfig.AACatalogPath))
            {
                Directory.Delete(PathConfig.AACatalogPath, true);
            }
        }
    }
}
