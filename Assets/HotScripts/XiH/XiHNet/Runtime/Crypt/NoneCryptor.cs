using System;
namespace XiHNet
{

    /// <summary>
    /// "空"加密
    /// </summary>
    public class NoneCryptor : ICryptor
    {
        public static NoneCryptor Default { get; } = new NoneCryptor();


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
            output = buffer;
            if (buffer == null || start < 0 || size < 0)
                return false;
            int end = start + size;
            if (end > buffer.Length)
                return false;
            return true;
        }
    }
}