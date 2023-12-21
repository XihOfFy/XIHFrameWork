namespace XiHNet
{
    /// <summary>
    /// 网络层加密/解密接口
    /// </summary>
    public interface ICryptor
    {
        bool Encrypt(byte[] buffer, int start, int size, out byte[] output);
        bool Decrypt(byte[] buffer, int start, int size, out byte[] output);
    }
}
