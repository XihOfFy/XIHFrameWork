#if UNITY_WX && !UNITY_EDITOR
#define UNITY_WX_WITHOUT_EDITOR
#endif
#if UNITY_WX_WITHOUT_EDITOR
using WeChatWASM;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aot.XiHUtil
{
    public class AotPlayerPrefsUtil
    {
        public const string GAME_RES_VERSION = nameof(GAME_RES_VERSION);
        public static void Set(string key, string val)
        {
#if UNITY_WX_WITHOUT_EDITOR
            WX.StorageSetStringSync(key, val);
#elif UNITY_HW_QG
            HWWASM.QG.LocalStorage.SetItem(key, val);
#elif UNITY_DY
            TTSDK.TT.PlayerPrefs.SetString(key, val);
            TTSDK.TT.PlayerPrefs.Save();
#elif UNITY_KS
            KSWASM.KS.StorageSetStringSync(key, val);
#else

            UnityEngine.PlayerPrefs.SetString(key, val);
            UnityEngine.PlayerPrefs.Save();
#endif
        }
        public static void Set(string key, bool val)
        {
            Set(key, val ? 1 : 0);
        }
        public static void Set(string key, int val)
        {
#if UNITY_WX_WITHOUT_EDITOR
            WX.StorageSetIntSync(key, val);
#elif UNITY_HW_QG
            HWWASM.QG.LocalStorage.SetItem(key, val.ToString());
#elif UNITY_DY
            TTSDK.TT.PlayerPrefs.SetInt(key, val);
            TTSDK.TT.PlayerPrefs.Save();
#elif UNITY_KS
            KSWASM.KS.StorageSetIntSync(key, val);
#else
            UnityEngine.PlayerPrefs.SetInt(key, val);
            UnityEngine.PlayerPrefs.Save();
#endif
        }
        public static void Set(string key, float val)
        {
#if UNITY_WX_WITHOUT_EDITOR
            WX.StorageSetFloatSync(key, val);
#elif UNITY_HW_QG
            HWWASM.QG.LocalStorage.SetItem(key, val.ToString());
#elif UNITY_DY
            TTSDK.TT.PlayerPrefs.SetFloat(key, val);
            TTSDK.TT.PlayerPrefs.Save();
#elif UNITY_KS
            KSWASM.KS.StorageSetFloatSync(key, val);
#else
            UnityEngine.PlayerPrefs.SetFloat(key, val);
            UnityEngine.PlayerPrefs.Save();
#endif
        }
        public static string Get(string key, string val = "")
        {
#if UNITY_WX_WITHOUT_EDITOR
            return WX.StorageGetStringSync(key, val);
#elif UNITY_HW_QG
            return HWWASM.QG.LocalStorage.GetItem(key);
#elif UNITY_DY
            return TTSDK.TT.PlayerPrefs.GetString(key, val);
#elif UNITY_KS
            return KSWASM.KS.StorageGetStringSync(key,val);
#else
            return UnityEngine.PlayerPrefs.GetString(key, val);
#endif
        }
        public static bool Get(string key, bool val = false)
        {
            return Get(key, val ? 1 : 0) == 1;
        }
        public static int Get(string key, int val = 0)
        {
#if UNITY_WX_WITHOUT_EDITOR
            return WX.StorageGetIntSync(key, val);
#elif UNITY_HW_QG
            int.TryParse(HWWASM.QG.LocalStorage.GetItem(key),out var res);
            return res;
#elif UNITY_DY
            return TTSDK.TT.PlayerPrefs.GetInt(key, val);
#elif UNITY_KS
            return KSWASM.KS.StorageGetIntSync(key, val);
#else
            return UnityEngine.PlayerPrefs.GetInt(key, val);
#endif
        }
        public static float Get(string key, float val = 0)
        {
#if UNITY_WX_WITHOUT_EDITOR
            return WX.StorageGetFloatSync(key, val);
#elif UNITY_HW_QG
            float.TryParse(HWWASM.QG.LocalStorage.GetItem(key), out var res);
            return res;
#elif UNITY_DY
            return TTSDK.TT.PlayerPrefs.GetFloat(key, val);
#elif UNITY_KS
            return KSWASM.KS.StorageGetFloatSync(key, val);
#else
            return UnityEngine.PlayerPrefs.GetFloat(key, val);
#endif
        }
        public static void DeleteKey(string key)
        {
#if UNITY_WX_WITHOUT_EDITOR
            WX.StorageDeleteKeySync(key);
#elif UNITY_HW_QG
            HWWASM.QG.LocalStorage.RemoveItem(key);
#elif UNITY_DY
            TTSDK.TT.PlayerPrefs.DeleteKey(key);
#elif UNITY_KS
            KSWASM.KS.StorageDeleteKeySync(key);
#else
            UnityEngine.PlayerPrefs.DeleteKey(key);
            UnityEngine.PlayerPrefs.Save();
#endif
        }
        public static void DeleteAllKey()
        {
#if UNITY_WX_WITHOUT_EDITOR
            WX.StorageDeleteAllSync();
#elif UNITY_HW_QG
            HWWASM.QG.LocalStorage.Clear();
#elif UNITY_DY
            TTSDK.TT.PlayerPrefs.DeleteAll();
#elif UNITY_KS
            KSWASM.KS.StorageDeleteAllSync();
#else
            UnityEngine.PlayerPrefs.DeleteAll();
            UnityEngine.PlayerPrefs.Save();
#endif
        }
        public static bool HasKey(string key)
        {
#if UNITY_WX_WITHOUT_EDITOR
            return WX.StorageHasKeySync(key);
#elif UNITY_HW_QG
            //HWWASM.QG.LocalStorage.StorageHasKeySync(key);
            var res = HWWASM.QG.LocalStorage.GetItem(key);
            Debug.LogError("华为不存在key的判断");
            return !string.IsNullOrEmpty(res);
#elif UNITY_DY
            return TTSDK.TT.PlayerPrefs.HasKey(key);
#elif UNITY_KS
            return KSWASM.KS.StorageHasKeySync(key);
#else
            return UnityEngine.PlayerPrefs.HasKey(key);
#endif
        }
    }
}
