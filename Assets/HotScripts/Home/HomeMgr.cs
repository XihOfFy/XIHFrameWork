using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hot
{
    public class HomeMgr : MonoBehaviour
    {
        private static HomeMgr instance;
        public static HomeMgr Instance => instance;
        private void Awake()
        {
            instance = this;
        }
        private void OnDestroy()
        {
            instance = null;
        }
    }
}
