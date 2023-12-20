using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using YooAsset;
using Aot;

namespace Hot
{
    public class HotMgr : MonoBehaviour
    {
        public TMP_Text tip;

        // Start is called before the first frame update
        void Start()
        {
            tip.text = "这里是热更场景111";
            YooAssets.GetPackage(AotConfig.PACKAGE_NAME).UnloadUnusedAssets();
        }
    }
}
