using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Hot
{
    public class HotMgr : MonoBehaviour
    {
        public TMP_Text tip;

        // Start is called before the first frame update
        void Start()
        {
            tip.text = "这里是热更场景";
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
