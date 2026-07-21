using System.Collections.Generic;
using System.Linq;
using Aot;
using Aot2Hot;
using Cysharp.Threading.Tasks;
using Tmpl;
#if UNITY_TT
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;
#endif
using UnityEngine;

namespace Hot
{
    public enum GameResultEnum
    {
        None = 0,//0	未知
        Pass = 1,//1	成功
        Fail = 2,//2	失败
        Quit = 3,//3	退出
        Restart = 4,//4	重玩
        FailRestart = 5,//5	失败重玩
    }
    public enum VideoSceneEnum
    {
        None = 0, //0	未知
        InsertAd,
    }
    public enum CoinObtainEnum
    {
        None = -1,
        Scene0 = 0,//0	胜利界面金币
        Scene1 = 1, //1	金币购买
        Pay = 999, //-1	内购
    }

    public class TrackingReport
    {
        #region 混合必接事件
        public static void TrackLoadComplete()
        {
#if UNITY_TT
            var jsonData=new JsonData();
            jsonData["timestamp"] = TimeUtils.GetCurrentTimeMs();
            TTReport("loading_complete", jsonData);
#endif
        }
        public static void TrackLevelEnd(bool pass, int level)
        {
#if UNITY_TT
            var jsonData = new JsonData();
            jsonData["section_type"] = "1";
            jsonData["main_section_no"] = 1;
            jsonData["section_value"] = (pass)?1:0;
            jsonData["section_name"] = "";
            jsonData["section_id"] = level;
            jsonData["section_sum"] = level;
            TTReport("complete_section", jsonData);
#endif
        }
        public static void TrackGainCredits(int num)
        {
#if UNITY_TT
            var jsonData = new JsonData();
            jsonData["value"] = num;
            jsonData["token_type"] = 1;
            jsonData["token_id"] = ItemEnum.Coin.ToString();
            TTReport("gain_credits", jsonData);
#endif
        }
        //离开游戏SDK内部已处理，无需额外处理

        #endregion

        #region TT用户埋点必接    SEEG/DY埋点
        //https://bytedance.sg.larkoffice.com/docx/Gtwbd2PxIo5axTx6kuxlicJugQh
        // 玩家离开游戏，SeegSDK 自带所以不用处理
        public static void TTUserLeave(int reason)
        {
#if UNITY_TT
            var jsonData = new JsonData();
            jsonData["leave_time"] = TimeUtils.GetCurrentTimeMs();
            jsonData["leave_reason"] = reason;
            TTReport("user_leave", jsonData);
#endif
        }
#if UNITY_TT
        static void TTReport(string name, JsonData jsonData) {
            TT.ReportEvent(new ReportEventParam()
            {
                eventName = name,
                @params = jsonData,
#if USE_GM
                fail = () =>
                {
                    Debug.LogError($"ReportEvent {name} fail");
                },
                success = () =>
                {
                    Debug.Log($"ReportEvent {name} success");
                },
#endif
            });
        }
#endif
        #endregion
    }
}
