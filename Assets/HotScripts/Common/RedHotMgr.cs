using System;
using System.Collections.Generic;
using XiHUtil;

namespace Hot
{
    public class RedDotMgr : Singleton<RedDotMgr>
    {
        private Dictionary<string, Action<int>> m_AllNodes = new Dictionary<string, Action<int>>();

        public void AddListener(string path, Action<int> callback) {
            if (callback == null)
            {
                return;
            }
            if (!m_AllNodes.ContainsKey(path))
            {
                m_AllNodes[path] = callback;
            }
            else {
                m_AllNodes[path] += callback;
            }
        }

        public void RemoveListener(string path, Action<int> callback)
        {
            if (callback == null)
            {
                return;
            }
            if (!m_AllNodes.ContainsKey(path)) { return; }
            m_AllNodes[path] -= callback;
        }

        public void RedNotify(string path, int num) { 
            if (!m_AllNodes.ContainsKey(path)) { return; }
            var action = m_AllNodes[path];
            if (action != null)
            {
                action.Invoke(num);
            }
        }
    }
}
