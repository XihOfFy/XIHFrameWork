using System;

namespace Aot.XiHUtil
{
    //无需实现IDecryptionServices，因为我们只加密原生文件rawfile，不是ab
    public class XIHDecryptionServices
    {
        public static byte[] EnOrDecryptDll(byte[] bytes)
        {
            int len = bytes.Length;
            var encryptedData = new byte[len];
            Buffer.BlockCopy(bytes, 0, encryptedData, 0, len);
            for (int i = 0; i < len; ++i)
            {
                encryptedData[i] ^= (byte)(i | len | 0x4251);
            }
            return encryptedData;
        }
        public static byte[] EnOrDecryptYooManifest(byte[] bytes)
        {
            int len = bytes.Length;
            var encryptedData = new byte[len];
            Buffer.BlockCopy(bytes, 0, encryptedData, 0, len);
            for (int i = 0; i < len; ++i)
            {
                encryptedData[i] ^= (byte)(i | len | 0x5564);
            }
            return encryptedData;
        }
        public static byte[] EncryptYooAB(byte[] bytes, int offset)
        {
            int len = bytes.Length;
            var newBytes = new byte[len + offset];
            var half = (len >> 1);
            for (int i = 0; i < offset; ++i)
            {
                if (i < half) newBytes[i] = bytes[i];
                else newBytes[i] = (byte)((offset | i) % 0XF);
            }
            Array.Copy(bytes, 0, newBytes, offset, len);
            return newBytes;
        }
        public static byte[] DecryptYooAB(byte[] bytes, int offset)
        {
            int len = bytes.Length;
            var newBytes = new byte[len - offset];
            Array.Copy(bytes, offset, newBytes, 0, len - offset);
            return newBytes;
        }
    }
}
