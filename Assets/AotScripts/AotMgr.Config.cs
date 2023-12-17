using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using System;

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
            tip.text = "InitConfigStart" + tryTime;

            var www = UnityWebRequest.Get(AotConfig.GetFrontUrl());
            www.certificateHandler = new CertificateHandlerImpl();
            try
            {
                var result = await www.SendWebRequest().WithCancellation(this.GetCancellationTokenOnDestroy());
                if (string.IsNullOrEmpty(www.error))
                {
                    var json = result.downloadHandler.text;
                    AotConfig.frontConfig = JsonUtility.FromJson<FrontConfig>(json);
                    InitYooAssetStart().Forget();
                }
            }
            catch (Exception)
            {
                if (--tryTime > 0)
                {
                    InitConfigStart(tryTime).Forget();
                    Debug.LogError($"剩余尝试次数:{tryTime}");
                }
                else
                {
                    QuitGame();
                }
            }
            finally {
                www.certificateHandler.Dispose();
                www.Dispose();
            }
        }
    }
}
