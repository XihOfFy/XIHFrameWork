using Cysharp.Threading.Tasks;
using Obfuz;
using Obfuz.EncryptionVM;
using UnityEngine;

namespace Aot
{
    public partial class AotMgr
    {
        // 初始化EncryptionService后被混淆的代码才能正常运行，
        // 因此尽可能地早地初始化它。
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void SetUpStaticSecretKey()
        {
            EncryptionService<DefaultStaticEncryptionScope>.Encryptor = new GeneratedEncryptionVirtualMachine(Resources.Load<TextAsset>("Obfuz/defaultStaticSecretKey").bytes);
        }
        private async UniTask SetUpDynamicSecret()
        {
            var rawOp = AssetLoadUtil.LoadAssetAsync<TextAsset>("Assets/Res/Aot2Hot/Obfuz/defaultDynamicSecretKey.bytes");
            await rawOp.ToUniTask();
            EncryptionService<DefaultDynamicEncryptionScope>.Encryptor = new GeneratedEncryptionVirtualMachine(rawOp.GetAsset<TextAsset>().bytes);
            rawOp.Release();
        }
    }
}
