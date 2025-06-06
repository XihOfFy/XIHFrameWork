using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using System;
using YooAsset;
#if UNITY_WX
using WeChatWASM;
#endif
namespace Aot
{
    public partial class AotMgr
    {
        class CertificateHandlerImpl : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
            }
        }
        //这里通过服务器获取热更地址配置信息
        async UniTaskVoid InitConfigStart(int tryTime)
        {
            var www = UnityWebRequest.Get(AotConfig.GetFrontUrl());
            www.certificateHandler = new CertificateHandlerImpl();
            try
            {
                var result = await www.SendWebRequest().WithCancellation(this.GetCancellationTokenOnDestroy());
                if (string.IsNullOrEmpty(www.error))
                {
                    var json = result.downloadHandler.text;
                    Debug.Log($"{www.url}返回：{json}");
                    AotConfig.frontConfig = JsonUtility.FromJson<FrontConfig>(json);
#if UNITY_WX && !UNITY_EDITOR
                    //Debug.LogWarning($"设置微信小游戏的CDN为:{AotConfig.frontConfig.cdn}");
                    WX.SetDataCDN(AotConfig.frontConfig.cdn);
#endif
                    InitYooAssetStart().Forget();
                }
            }
            catch (Exception e)
            {
                if (--tryTime > 0)
                {
                    Debug.LogError($"剩余尝试次数:{tryTime} >> {www.uri} \n{e}");
                    InitConfigStart(tryTime).Forget();
                }
                else
                {
                    if (playMode == EPlayMode.HostPlayMode)
                    {
                        //主机模式下尝试本地进入游戏，弱网游戏
                        InitYooAssetStart().Forget();
                    }
                    else
                    {
                        QuitGame();//强联网，尝试多次失败直接退出游戏，不让玩
                    }
                }
            }
            finally {
                www.certificateHandler.Dispose();
                www.Dispose();
            }
        }
    }
}
