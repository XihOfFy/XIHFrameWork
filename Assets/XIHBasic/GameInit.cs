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
            //比对版本高低自行实现，目前暂定版本不同就更新，不关心版本高低
            string urlPath = $"{PlatformConfig.PersistentDataPath}/{PlatformConfig.CONFIG_NAME}";
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
                File.Delete(urlPath);
            }
            byte[] pdb = null;
            string pdbPath = $"{PlatformConfig.PersistentDataPath}/{PlatformConfig.HOTFIX_DLL_NAME}.pdb";
            if (File.Exists(pdbPath))
            {
                pdb = File.ReadAllBytes(pdbPath);
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
