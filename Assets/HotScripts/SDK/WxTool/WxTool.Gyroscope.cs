#if UNITY_WX
using WeChatWASM;
#endif
using UnityEngine;
using System;
using Tmpl;

namespace Hot
{
    public static partial class WxTool
    {
        private static Action<Vector3> GyroscopeChangeAct;
        public static void StartGyroscopeChange(Action<Vector3> action)
        {
/*            if (DataSave.Instance.stageId < Tables.Instance.TbParam.SensorStage) return;
            StopGyroscopeChange();
            GyroscopeChangeAct = action;
#if UNITY_WX
            WX.StartGyroscope(new StartGyroscopeOption()
            {
                interval = "normal",
                fail = res => Debug.LogError(res.errMsg)
            });
            WX.OffGyroscopeChange(OnGyroscopeChange);
            WX.OnGyroscopeChange(OnGyroscopeChange);
#endif*/
        }
#if UNITY_WX
        private static void OnGyroscopeChange(OnGyroscopeChangeListenerResult result)
        {
            GyroscopeChangeAct?.Invoke(new Vector3((float)result.x, (float)result.y, (float)result.z));
        }
#endif
        public static void StopGyroscopeChange()
        {
/*            if (DataSave.Instance.stageId < Tables.Instance.TbParam.SensorStage) return;
#if UNITY_WX
            GyroscopeChangeAct = null;
            WX.OffGyroscopeChange(OnGyroscopeChange);
            WX.StopGyroscope(new StopGyroscopeOption() {
                fail = res => Debug.LogError(res.errMsg)
            });
#endif*/
        }
    }
}
