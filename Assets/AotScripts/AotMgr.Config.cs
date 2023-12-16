using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using System;

namespace Aot
{
    public partial class AotMgr
    {
        //这里通过服务器获取热更地址配置信息
        async UniTaskVoid InitConfigStart(int tryTime)
        {
            var www = UnityWebRequest.Get(AotConfig.GetFrontUrl());
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
                www.Dispose();
            }
        }
    }
}
