using Cysharp.Threading.Tasks;
using System;
using System.IO;
using UnityEngine;
using YooAsset;

namespace Aot
{
    public partial class AotMgr
    {
        //为了保持全平台一致逻辑，所以都使用webgl 小游戏的处理方式，不需要首包资源，全部通过下载
        async UniTaskVoid InitYooAssetStart()
        {
            // 初始化资源系统
            YooAssets.Initialize();

            // 创建资源包裹类
            var package = YooAssets.CreatePackage(AotConfig.PACKAGE_NAME);

            // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
            YooAssets.SetDefaultPackage(package);

            // 编辑器下的模拟模式
            InitializeParameters createParameters = null;
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                createParameters = new EditorSimulateModeParameters()
                {
                    SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.ScriptableBuildPipeline.ToString(), AotConfig.PACKAGE_NAME)
                };
            }
            // 单机运行模式
            else if (playMode == EPlayMode.OfflinePlayMode)
            {
                createParameters = new OfflinePlayModeParameters();
            }
            // 联机运行模式
            else if (playMode == EPlayMode.HostPlayMode)
            {
                createParameters = new HostPlayModeParameters()
                {
                    RemoteServices = new RemoteServices(),
                    BuildinQueryServices = new BuildinQueryServices()
                };
            }
            // WebGL运行模式
            else if (playMode == EPlayMode.WebPlayMode)
            {
                createParameters = new WebPlayModeParameters()
                {
                    RemoteServices = new RemoteServices(),
                    BuildinQueryServices = new BuildinQueryServices()
                };
                //因为微信小游戏平台的特殊性，需要关闭WebGL的缓存系统，使用微信自带的缓存系统。
                YooAssets.SetCacheSystemDisableCacheOnWebGL();
            }
            //web 不支持ab加密，所以对于原始资源，我们自行加密，然后打ab
            //createParameters.DecryptionServices = new DecryptionServices();

            var initializationOperation = package.InitializeAsync(createParameters);
            await initializationOperation.ToUniTask();
            // 如果初始化失败弹出提示界面
            if (initializationOperation.Status != EOperationStatus.Succeed)
            {
                QuitGame();//AOT启动过程必须保持一切顺利，不然全部退出游戏
                return;
            }

            UpdatePackageVersionAsync(package).Forget();
        }
        //解密接口            
        //web 不支持ab加密，所以对于原始资源，我们自行加密，然后打ab
        //这个也用不上了
        private class DecryptionServices : IDecryptionServices
        {
            public AssetBundle LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
            {
                /*if (fileInfo.BundleName.EndsWith(".rawfile"))
                {
                    EncryptResult result = new EncryptResult();
                    result.Encrypted = true;
                    result.EncryptedData = XIHDecryptionServices.ProcessRawFile(File.ReadAllBytes(fileInfo.FileLoadPath));
                }
                else
                {
                    EncryptResult result = new EncryptResult();
                    result.Encrypted = false;
                }*/
                managedStream = new MemoryStream(XIHDecryptionServices.Decrypt(File.ReadAllBytes(fileInfo.FileLoadPath))) ;
                return AssetBundle.LoadFromStream(managedStream);
            }

            public AssetBundleCreateRequest LoadAssetBundleAsync(DecryptFileInfo fileInfo, out Stream managedStream)
            {
                managedStream = new MemoryStream(XIHDecryptionServices.Decrypt(File.ReadAllBytes(fileInfo.FileLoadPath)));
                return AssetBundle.LoadFromStreamAsync(managedStream);
            }
        }
        /// <summary>
        /// 远端资源地址查询服务类
        /// </summary>
        private class RemoteServices : IRemoteServices
        {
            string IRemoteServices.GetRemoteMainURL(string fileName)
            {
                return AotConfig.frontConfig.defaultHostServer + "/"+ fileName;
            }
            string IRemoteServices.GetRemoteFallbackURL(string fileName)
            {
                return AotConfig.frontConfig.fallbackHostServer + "/" + fileName; 
            }
        }
        /// <summary>
        /// 小游戏启动时加载StreamingAsset内资源会被转换为www请求，此时走小游戏缓存可直接返回之前下载的资源
        /// 微信小游戏对于CDN下，带有StreamingAsset的资源可以缓存，所以我们设置YooAsset的下载路径和小游戏读取内置路径的url保持一致即可，
        /// 这样即时首包不包含任何内容，我们触发正常的YooAsset下载，也会被微信缓存了
        /// 所以为了先缓存资源，游戏启动需要先完成资源更新，将远程StreamingAsset资源全部缓存到本地，后面就不需要再从远程获取了
        /// </summary>
        private class BuildinQueryServices : IBuildinQueryServices
        {
            public bool Query(string packageName, string fileName, string fileCRC)
            {
                //Debug.Log($"BuildinQueryServices {packageName} >> {fileName}");
                return false;
            }
        }

        //获取资源版本
        async UniTaskVoid UpdatePackageVersionAsync(ResourcePackage package)
        {
            var yooOp = package.UpdatePackageVersionAsync();
            var uniOp = yooOp.ToUniTask();
            await uniOp;
            if (uniOp.Status == UniTaskStatus.Succeeded)
            {
                //更新成功
                Debug.Log($"Updated package Version : {yooOp.PackageVersion}");
                UpdatePackageManifest(package, yooOp.PackageVersion).Forget();
            }
            else
            {
                QuitGame();
            }
        }
        //UpdatePackageManifest
        async UniTaskVoid UpdatePackageManifest(ResourcePackage package,string packageVersion)
        {
            var yooOp = package.UpdatePackageManifestAsync(packageVersion,true);
            var uniOp = yooOp.ToUniTask();
            await uniOp;
            if (uniOp.Status == UniTaskStatus.Succeeded)
            {
                //更新成功
                DownloadAot2HotRes(package).Forget();
            }
            else
            {
                QuitGame();
            }
        }
        //资源包下载,只下载aot2hot的tag资源，余下的到hot再继续下载，因为aot不提供ui功能
        async UniTaskVoid DownloadAot2HotRes(ResourcePackage package) {
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            var downloader = package.CreateResourceDownloader("Aot2Hot", downloadingMaxNum, failedTryAgain);

            //注册回调方法，这里AOT使用这个是避免裁剪，不然HOT找不到该方法了
            downloader.OnDownloadErrorCallback = OnDownloadError;
            downloader.OnDownloadProgressCallback = OnDownloadProgress;

            //没有需要下载的资源
            if (downloader.TotalDownloadCount == 0)
            {
                GotoAot2HotScene().Forget();
                return;
            }
            //开启下载
            downloader.BeginDownload();
            await downloader.ToUniTask();

            //检测下载结果
            if (downloader.Status == EOperationStatus.Succeed)
            {
                GotoAot2HotScene().Forget();
            }
            else
            {
                QuitGame();
            }
        }
        void OnDownloadError(string fileName, string error)
        {
            Debug.LogError($"OnDownloadError:{fileName} > {error}");
        }
        void OnDownloadProgress(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
        {
            Debug.LogWarning($"正在下载({currentDownloadCount}/{totalDownloadCount}): {(currentDownloadBytes >> 10)}KB/{(totalDownloadBytes >> 10)}KB");
        }
    }
}
