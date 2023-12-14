using System;
using System.IO;
using YooAsset;

public class XIHEncryptionServices : IEncryptionServices
{
    public EncryptResult Encrypt(EncryptFileInfo fileInfo)
    {
        if (fileInfo.BundleName.EndsWith(".rawfile"))
        {
            EncryptResult result = new EncryptResult();
            result.Encrypted = true;
            result.EncryptedData = ProcessRawFile(File.ReadAllBytes(fileInfo.FilePath));
            return result;
        }
        else
        {
            EncryptResult result = new EncryptResult();
            result.Encrypted =false;
            return result;
        }
    }
    static byte[] ProcessRawFile(byte[] bytes)
    {
        int len = bytes.Length;
        var encryptedData = new byte[len];
        Buffer.BlockCopy(bytes, 0, encryptedData, 0, len);
        for (int i = 0; i < len; ++i)
        {
            encryptedData[i] ^= (byte)(i | len);
        }
        return encryptedData;
    }
}