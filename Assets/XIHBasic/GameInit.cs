using System;
using System.Collections;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace XIHBasic
{
    public class GameInit : MonoBehaviour
    {
        private void Awake()
        {
            gameObject.name = "XIHBaseEnter";
            DontDestroyOnLoad(this.gameObject);
            StartHotFix();
        }
        void StartHotFix()
        {
            string urlPath = $"{PlatformConfig.PersistentDataPath}/{PlatformConfig.CONFIG_NAME}";
            if (!File.Exists(urlPath))
            {
                File.WriteAllText(urlPath, Resources.Load<TextAsset>(PlatformConfig.CONFIG_NAME).text);
            }
            byte[] dll;
            string dllPath = $"{PlatformConfig.PersistentDataPath}/{PlatformConfig.HOTFIX_DLL_NAME}";
            if (File.Exists(dllPath))
            {
                dll = File.ReadAllBytes(dllPath);
            }
            else
            {
                dll = Resources.Load<TextAsset>(PlatformConfig.HOTFIX_DLL_NAME).bytes;
                File.WriteAllBytes(dllPath, dll);
            }
            byte[] pdb = null;
            if (File.Exists($"{PlatformConfig.PersistentDataPath}/{PlatformConfig.HOTFIX_DLL_NAME}.pdb"))
            {
                pdb = File.ReadAllBytes($"{PlatformConfig.PersistentDataPath}/{PlatformConfig.HOTFIX_DLL_NAME}.pdb");
            }
            if (!HotFixBridge.Start(dll, pdb))
            {
                File.Delete(urlPath);
                File.Delete(dllPath);
                Application.Quit();
            }
        }
        private void Update()
        {
            HotFixBridge.Update();
        }
        private void FixedUpdate()
        {
            HotFixBridge.FixedUpdate();
        }
        private void OnApplicationFocus(bool focus)
        {
            HotFixBridge.OnApplicationFocus(focus);
        }
        private void OnApplicationPause(bool pause)
        {
            HotFixBridge.OnApplicationPause(pause);
        }
        private void OnApplicationQuit()
        {
            HotFixBridge.ShutDown();
        }
    }
}
