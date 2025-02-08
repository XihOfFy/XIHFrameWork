#if UNITY_DY
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;
using LitJson;
namespace Hot
{

    /// <summary>
    /// 抖音SDK
    /// </summary>
    public class DYStarkSDK : IChannelSDK
    {
        public bool DY_PC { get; private set; }

        public void Init(Action<bool> initCallback)
        {
            //抖音vconsloe只有webgl模式才有
            TT.InitSDK((code, env) => {
                UnityEngine.Debug.Log($"InitSDK... code: {code}");
                initCallback?.Invoke(true);//自己写结果判断
            });
            TT.GetAppLifeCycle().OnShow = (param) =>
            {
                SoundMgr.Instance.UnPause();
            };
            TT.GetAppLifeCycle().OnHide = () => {
                SoundMgr.Instance.PauseBGM();
            };
            var device = TT.GetSystemInfo();
            DY_PC = "windows".Equals(device.platform);
        }

        public void TouchOverride(GameObject dotDestoryObj)
        {
            //https://developer.open-douyin.com/docs/resource/zh-CN/mini-game/develop/guide/game-engine/rd-to-SCgame/open-capacity/capability-adaptation/sc-webgl-touch
            dotDestoryObj.AddComponent<StarkSDKSpace.StarkInputOverrideBypass>();//需要添加宏定义 STARK_UNITY_INPUT_OVERRIDE
            FairyGUI.Stage.touchScreen = true; // 在首次触控操作前调用，否则 Began 事件会丢失。
        }

        /*public void CallSDKShare(Share share)
        {
            ShareInfo shareInfo = new ShareInfo();
            shareInfo.title = share.title;
            shareInfo.content = share.content;
            shareInfo.imgPath = share.imgPath;
            shareInfo.imgUrl = share.imgUrl;
            shareInfo.url = share.url;
            shareInfo.type = share.type;
            shareInfo.shareTo = share.shareTo;
            shareInfo.extenal = share.extenal;


            var shareJson = new JsonData();
            shareJson["channel"] = "video";
            shareJson["title"] = share.title;
            shareJson["extra"] = new JsonData();
            shareJson["extra"]["videoPath"] = share.url;//OnRecordComplete拿到 录屏文件 所在路径
            //JsonData videoTopics = new JsonData();
            //videoTopics.SetJsonType(JsonType.Array);
            //videoTopics.Add("Some Topic1");
            //videoTopics.Add("Some Topic2");
            //shareJson["extra"]["videoTopics"] = videoTopics;
            //shareJson["extra"]["hashtag_list"] = videoTopics;
            TT.ShareAppMessage(shareJson ,(data) =>
            {
                // Share succeed
                Debug.Log($"分享成功 {data}");
            }, (errMsg) =>
            {
                // Share failed
                Debug.Log($"分享失败 {errMsg}");
            }, () =>
            {
                // Share cancelled
                Debug.Log($"取消分享");
            });
        }*/

        public void StartRecord()
        {
            if (TT.GetGameRecorder().GetVideoRecordState() !=TTGameRecorder.VideoRecordState.RECORD_STARTED)
            {
                TT.GetGameRecorder().Start(true, 0, 
                    () => {
                        UnityEngine.Debug.Log("开始录制");
                        _listener.OnRecordEndCallback(VideoRecordStatus.RECORD_ING, "");
                    }, 
                    (int errCode, string errMsg) => {
                        UnityEngine.Debug.Log($"录制失败 {errCode} {errMsg}");
                        _listener.OnRecordEndCallback(VideoRecordStatus.RECORD_ERROR, errMsg);
                    }, (string videoPath) => {
                        UnityEngine.Debug.Log($"录制超时 {videoPath}");
                        _listener.OnRecordEndCallback(VideoRecordStatus.RECORD_END, videoPath);
                    });
            }
            else
            {
                UnityEngine.Debug.Log("已经在录制中");
            }
        }

        public void StopRecord()
        {
            var recordState = TT.GetGameRecorder().GetVideoRecordState();
            if (recordState == TTGameRecorder.VideoRecordState.RECORD_STARTING ||
                recordState == TTGameRecorder.VideoRecordState.RECORD_STARTED ||
                recordState == TTGameRecorder.VideoRecordState.RECORD_PAUSING ||
                recordState == TTGameRecorder.VideoRecordState.RECORD_PAUSED)
            {
                TT.GetGameRecorder().Stop( (string videoPath) => {
                    _listener.OnRecordEndCallback(VideoRecordStatus.RECORD_END, videoPath);
                    });
            }
            else
            {
                _listener.OnRecordEndCallback(VideoRecordStatus.NONE, "");
            }
        }
        public void OpenCustomerServicePage()
        {
            TT.OpenCustomerServicePage(
            (flag) =>
            {
                UnityEngine.Debug.Log($"打开当前客服状态: {flag}");
            });
        }


        public void SetImRankData(int chapterId, int stageId)
        {
            var priority = stageId;
            var paramJson = new JsonData
            {
                ["value"] = $"{stageId}",
                ["dataType"] = 0,
            };
            TT.SetImRankData(paramJson, (isSuccess, errMsg) =>
            {
                UnityEngine.Debug.Log($"写入排行榜：{isSuccess} {errMsg}");
            });
        }
        public void GetImRankList()
        {
            var paramJson = new JsonData
            {
                ["rankType"] = "day",
                ["relationType"] = "default",
                ["dataType"] = 0,
                ["rankTitle"] = "每日排行榜",
                ["suffix"] = "关"
            };
            TT.GetImRankList(paramJson, (isSuccess, errMsg) =>
            {
                UnityEngine.Debug.Log($"获取排行榜：{isSuccess} {errMsg}");
            });
        }

        public void AddShortcut()
        {
            TT.AddShortcut((isSuccess) =>
            {
                UnityEngine.Debug.Log($"添加快捷方式：{isSuccess}");
            });
        }

        public void SetClipboardData(string content)
        {
            TT.SetClipboardData(content);
        }
        public UniTask<bool> NeedShowSideBar()
        {
            var tcs = new UniTaskCompletionSource<bool>();
            if (DY_PC) {
                tcs.TrySetResult(false);
                return tcs.Task;
            }
            TT.CheckScene(TTSideBar.SceneEnum.SideBar, res => {
                tcs.TrySetResult(res);
            }, () => { }, (code, msg) => {
                tcs.TrySetResult(false);
                Debug.LogError($"CheckScene {code}:{msg}");
            });
            return tcs.Task;
        }
        public void NavigateToSideBarScene()
        {
            var jsonData = new JsonData();
            jsonData["scene"] = "sidebar";
            jsonData["activity"] = "";
            TT.NavigateToScene(jsonData, () => { }, () => { }, (code, msg) => {
                Debug.LogError($"NavigateToScene {code}:{msg}");
            });
        }
    }
}
#endif
