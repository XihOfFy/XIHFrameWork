using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace Aot
{
    public partial class AotMgr
    {
        const string PACKAGE_NAME = "DefaultPackage";
        //为了保持全平台一致逻辑，所以都使用webgl 小游戏的处理方式，不需要首包资源，全部通过下载
        async UniTaskVoid InitYooAssetStart() {
            // 初始化资源系统
            YooAssets.Initialize();
            // 创建资源包裹类
            var package = YooAssets.CreatePackage(PACKAGE_NAME);
            // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
            YooAssets.SetDefaultPackage(package);

            // 编辑器下的模拟模式
            InitializeParameters createParameters = null;
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                createParameters = new EditorSimulateModeParameters()
                {
                    SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.ScriptableBuildPipeline.ToString(), PACKAGE_NAME)
                };
            }
            // 单机运行模式
            else if (playMode == EPlayMode.OfflinePlayMode)
            {
                createParameters = new OfflinePlayModeParameters();
            }
            // 联机运行模式
            else if(playMode == EPlayMode.HostPlayMode)
            {
                createParameters = new HostPlayModeParameters() {
                    RemoteServices = new RemoteServices()
                };
            }
            // WebGL运行模式
            else if(playMode == EPlayMode.WebPlayMode)
            {
                createParameters = new WebPlayModeParameters() {
                    RemoteServices = new RemoteServices()
                };
            }

            var initializationOperation = package.InitializeAsync(createParameters);
            await initializationOperation.ToUniTask();

            // 如果初始化失败弹出提示界面
            if (initializationOperation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning($"{initializationOperation.Error}");
            }
            var version = package.GetPackageVersion();
            Debug.Log($"Init resource package version : {version}");

        }

        /// <summary>
        /// 远端资源地址查询服务类
        /// </summary>
        private class RemoteServices : IRemoteServices
        {
            string IRemoteServices.GetRemoteMainURL(string fileName)
            {
                return AotConfig.frontConfig.defaultHostServer;
            }
            string IRemoteServices.GetRemoteFallbackURL(string fileName)
            {
                return AotConfig.frontConfig.fallbackHostServer;
            }
        }
    }
}
