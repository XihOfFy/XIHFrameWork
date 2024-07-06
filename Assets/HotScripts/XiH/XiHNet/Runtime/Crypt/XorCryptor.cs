using System;
namespace XiHNet
{

    /// <summary>
    /// 简单异或加密
    /// </summary>
    public class XorCryptor : ICryptor
    {
        private readonly byte[] _key;
        private readonly int _keyLen;

        public XorCryptor(byte[] key)
        {
            _key = key;
            _keyLen = key.Length;
        }
        public bool Encrypt(byte[] buffer, int start, int size, out byte[] output)
        {
            return Process(buffer, start, size, out output);
        }

        public bool Decrypt(byte[] buffer, int start, int size, out byte[] output)
        {
            return Process(buffer, start, size, out output);
        }

        private bool Process(byte[] buffer, int start, int size, out byte[] output)
        {
            output = new byte[size];
            if (buffer == null || start < 0 || size < 0)
                return false;
            int end = start + size;
            if (end > buffer.Length)
                return false;
            if (size > 0)
            {
                for (int i = start, j = 0; i < end; ++i, ++j)
                    output[j] = (byte)(buffer[i] ^ _key[j % _keyLen]);
            }

            return true;
        }
    }
}