#if UNITY_WX && !UNITY_EDITOR
#define UNITY_WX_WITHOUT_EDITOR
#endif
#if UNITY_WX
using WeChatWASM;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XiHUtil
{
    public class PlayerPrefsUtil
    {
        public static void Set(string key, string val) {
#if UNITY_WX_WITHOUT_EDITOR
            WX.StorageSetStringSync(key, val);
#else
            PlayerPrefs.SetString(key, val);
            PlayerPrefs.Save();
#endif
        }
        public static void Set(string key, int val)
        {
#if UNITY_WX_WITHOUT_EDITOR
            WX.StorageSetIntSync(key, val);
#else
            PlayerPrefs.SetInt(key, val);
            PlayerPrefs.Save();
#endif
        }
        public static void Set(string key, float val)
        {
#if UNITY_WX_WITHOUT_EDITOR
            WX.StorageSetFloatSync(key, val);
#else
            PlayerPrefs.SetFloat(key, val);
            PlayerPrefs.Save();
#endif
        }
        public static string Get(string key, string val="")
        {
#if UNITY_WX_WITHOUT_EDITOR
            return WX.StorageGetStringSync(key, val);
#else
            return PlayerPrefs.GetString(key, val);
#endif
        }
        public static int Get(string key, int val=0)
        {
#if UNITY_WX_WITHOUT_EDITOR
            return WX.StorageGetIntSync(key, val);
#else
            return PlayerPrefs.GetInt(key, val);
#endif
        }
        public static float Get(string key, float val=0)
        {
#if UNITY_WX_WITHOUT_EDITOR
            return WX.StorageGetFloatSync(key, val);
#else
            return PlayerPrefs.GetFloat(key, val);
#endif
        }
        public static void DeleteKey(string key)
        {
#if UNITY_WX_WITHOUT_EDITOR
            WX.StorageDeleteKeySync(key);
#else
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
#endif
        }
        public static void DeleteAllKey()
        {
#if UNITY_WX_WITHOUT_EDITOR
            WX.StorageDeleteAllSync();
#else
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
#endif
        }
        public static bool HasKey(string key) {
#if UNITY_WX_WITHOUT_EDITOR
            return WX.StorageHasKeySync(key);
#else
            return PlayerPrefs.HasKey(key);
#endif
        }
    }
}
