using UnityEngine;
using YooAsset;

namespace Aot
{
    public partial class AotMgr : MonoBehaviour
    {
        public EPlayMode playMode = EPlayMode.EditorSimulateMode;
        private void Awake()
        {
            if (EPlayMode.WebPlayMode == playMode || EPlayMode.HostPlayMode == playMode)
            {
                InitConfigStart(8).Forget();
            }
            else
            {//非联机模式直接跳到yooasset初始化
                InitYooAssetStart().Forget();
            }
        }
    }
}
