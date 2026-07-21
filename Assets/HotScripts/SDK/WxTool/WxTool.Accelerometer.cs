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
        private static Action<Vector3> AccelerometerChangeAct;
        public static void StartAccelerometer(Action<Vector3> action)
        {
/*            if (DataSave.Instance.stageId < Tables.Instance.TbParam.SensorStage) return;
            StopAccelerometer();
            AccelerometerChangeAct = action;
#if UNITY_WX
            WX.StartAccelerometer(new StartAccelerometerOption()
            {
                interval = "normal",
                fail = res => Debug.LogError(res.errMsg)
            });
            WX.OffAccelerometerChange(OnAccelerometerChange);
            WX.OnAccelerometerChange(OnAccelerometerChange);
#endif*/
        }
#if UNITY_WX
        private static void OnAccelerometerChange(OnAccelerometerChangeListenerResult result)
        {
            AccelerometerChangeAct?.Invoke(new Vector3((float)result.x, (float)result.y, (float)result.z));
        }
#endif
        public static void StopAccelerometer()
        {
/*            if (DataSave.Instance.stageId < Tables.Instance.TbParam.SensorStage) return;
#if UNITY_WX
            AccelerometerChangeAct = null;
            WX.OffAccelerometerChange(OnAccelerometerChange);
            WX.StopAccelerometer(new StopAccelerometerOption() {
                fail = res => Debug.LogError(res.errMsg)
            });
#endif*/
        }
    }
}
