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
                var result = await www.SendWebRequest();
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
                    Application.Quit();//尝试多次失败直接退出游戏，不让玩
#if UNITY_EDITOR
                    Debug.LogError($"该报错解决方法：在{nameof(AotConfig.GetFrontUrl)}和{nameof(AotConfig.InitFrontConfig)}方法修改为你本地的web地址；\n Windows下菜单栏 XIHUtil/Server/WebSvr 即可开启本地web服务；\n Mac用户请自行搭建web服务，且设置web根路径为 XIHWebServerRes (与Assets同层级)");
#endif
                }
            }
            finally {
                www.Dispose();
            }
        }
    }
}
