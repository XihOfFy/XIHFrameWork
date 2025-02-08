#if UNITY_WX
using System;
using UnityEngine;
using WeChatWASM;
using XiHSound;
namespace Hot
{
    class WXSDK : IChannelSDK
    {
        public void TouchOverride(GameObject dotDestoryObj)
        {
            dotDestoryObj.AddComponent<WXTouchInputOverride>();//这里会涉及隐私WX.GetSystemInfoSync().platform，所以建议放在CheckPrivacy之后执行，且这里还会执行一次WX.InitSDK
        }

        void IChannelSDK.Init(Action<bool> initCallback)
        {
            WX.InitSDK(code => {
                UnityEngine.Debug.Log($"InitSDK... code: {code}");
                initCallback?.Invoke(true);//自己写结果判断
            });
            //1.8.0
            // 监听音频因为受到系统占用而被中断开始事件。以下场景会触发此事件：闹钟、电话、FaceTime 通话、微信语音聊天、微信视频聊天、有声广告开始播放、实名认证页面弹出等。此事件触发后，小程序内所有音频会暂停。
            WX.OnAudioInterruptionBegin(res => {
                SoundMgr.Instance.PauseBGM();
            });
            //监听音频中断结束事件。在收到 onAudioInterruptionBegin 事件之后，小程序内所有音频会暂停，收到此事件之后才可再次播放成功
            WX.OnAudioInterruptionEnd(res => {
                SoundMgr.Instance.UnPause();
            });
            WeChatWASM.WX.OnHide(res => {
                SoundMgr.Instance.PauseBGM();
            });
            WeChatWASM.WX.OnShow(res => {
                SoundMgr.Instance.UnPause();
            });
        }
    }
}
#endif