using System;
using System.IO;
using UnityEngine;
using Aot.XiHUtil;
public class XIHEncryptionServices
{
    public static void Encrypt(string srcFilePath, string dstFilePath)
    {
        if (!File.Exists(srcFilePath))
        {
            Debug.LogError($"CopyDll {srcFilePath}不存在 (这个报错若是Hot.Ext.dll可以忽略，因为这个是预留的，所以不存在很正常)");
            return;
        }
        File.WriteAllBytes(dstFilePath, XIHDecryptionServices.Decrypt(File.ReadAllBytes(srcFilePath)));
        Debug.LogWarning($"{srcFilePath}已经加密输出到{dstFilePath}");
    }
}