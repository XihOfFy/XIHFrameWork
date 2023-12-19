using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using YooAsset;

namespace Hot
{
    public class HotMgr : MonoBehaviour
    {
        string PACKAGE_NAME = "DefaultPackage";
        public TMP_Text tip;

        // Start is called before the first frame update
        void Start()
        {
            tip.text = "这里是热更场景";
            YooAssets.GetPackage(PACKAGE_NAME).UnloadUnusedAssets();
        }
    }
}
