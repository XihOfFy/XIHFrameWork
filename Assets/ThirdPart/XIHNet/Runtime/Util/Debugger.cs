#define OPENLOG
#if OPENLOG
#if XIHSERVER
using XIHServer;
#else
using UnityEngine;
#endif
#endif
namespace XiHNet
{
    public static class Debugger
    {
        public static void Log(string log)
        {
#if OPENLOG
#if XIHSERVER
            XIHLog4Net.Info(log);
#else
            UnityEngine.Debug.Log(log);
#endif
#endif
        }
    }
}
