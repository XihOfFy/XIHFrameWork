using Aot;
using UnityEngine;
using YooAsset;

namespace Hot
{
    public class HomeMgr : MonoBehaviour
    {
        private static HomeMgr instance;
        public static HomeMgr Instance => instance;
        private void Awake()
        {
            instance = this;
            YooAssets.GetPackage(AotConfig.PACKAGE_NAME).UnloadUnusedAssets();
        }
        private void OnDestroy()
        {
            instance = null;
        }
    }
}
