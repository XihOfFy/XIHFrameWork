using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XIHBasic
{
    /// <summary>
    /// The First Call Must in MainThread
    /// </summary>
    public sealed class PlatformConfig
    {
        public const string PLATFORM_NAME =
#if UNITY_ANDROID
            "Android";
#elif UNITY_IOS
            "iOS";
#else
            "Other";
#endif
        public const string CONFIG_NAME = "UrlConfig";
        public const string HOTFIX_DLL_NAME = "XIHHotFix";
        public static string PersistentDataPath { get; }
        static PlatformConfig()
        {
            PersistentDataPath = Application.persistentDataPath;//MainThread
        }
    }
}
