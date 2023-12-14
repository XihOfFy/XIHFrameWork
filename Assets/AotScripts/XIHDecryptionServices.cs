using System;

namespace Aot
{
    //无需实现IDecryptionServices，因为我们只加密原生文件rawfile，不是ab
    public class XIHDecryptionServices
    {
        public static byte[] ProcessRawFile(byte[] bytes)
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
}
