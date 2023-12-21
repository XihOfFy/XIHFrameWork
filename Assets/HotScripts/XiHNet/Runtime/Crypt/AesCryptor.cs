using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace XiHNet
{
    public class AesCryptor : ICryptor
    {
        private readonly byte[] _key;

        public AesCryptor(byte[] key)
        {
            _key = key;
        }

        public bool Decrypt(byte[] buffer, int start, int size, out byte[] output)
        {
            return Process(buffer, start, size, out output, false);
        }

        public bool Encrypt(byte[] buffer, int start, int size, out byte[] output)
        {
            return Process(buffer, start, size, out output, true);
        }

        private bool Process(byte[] buffer, int start, int size, out byte[] output, bool isEncrypt)
        {
            output = null;
            if (buffer == null || start < 0 || size < 0)
                return false;
            int end = start + size;
            if (end > buffer.Length)
                return false;
            using (Aes encryptor = Aes.Create())
            {
                encryptor.Mode = CipherMode.CBC;
                //encryptor.Mode = CipherMode.CFB;//.Net5.x与低版本框架不兼容
                encryptor.Key = _key;
                //encryptor.IV = new byte[16] {  76, 156, 114, 226, 131, 245, 190, 137, 241, 141, 178, 42, 44, 19, 153, 128  };
				encryptor.IV = new byte[16] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
                ICryptoTransform aesCryptorTr = isEncrypt ? encryptor.CreateEncryptor() : encryptor.CreateDecryptor();
                using (MemoryStream memoryStream = new MemoryStream())
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesCryptorTr, CryptoStreamMode.Write)) {
                    cryptoStream.Write(buffer, start, size);
                    cryptoStream.FlushFinalBlock();
                    output = memoryStream.ToArray();
                }
                return output != null;
            }
        }
        /*private bool Process(byte[] buffer, int start, int size, out byte[] output, bool isEncrypt)
        {
            using (RijndaelManaged Aes256 = new RijndaelManaged())
            {
                Aes256.BlockSize = 256;
                Aes256.KeySize = 256;
                Aes256.Mode = CipherMode.CFB;
                Aes256.FeedbackSize = 8;
                Aes256.Padding = PaddingMode.None;
                Aes256.Key = _key;
                Aes256.IV = new byte[16] { 76, 156, 114, 226, 131, 245, 190, 137, 241, 141, 178, 42, 44, 19, 153, 128 };
                using (var encryptor = isEncrypt ? Aes256.CreateEncryptor() : Aes256.CreateDecryptor())
                using (var memoryStream = new MemoryStream())
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(buffer, start, size);
                    cryptoStream.FlushFinalBlock();
                    output = memoryStream.ToArray();
                }
            }
            return output != null;
        }*/


    }
}
