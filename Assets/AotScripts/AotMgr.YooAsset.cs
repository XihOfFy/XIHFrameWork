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

                var buildResult = EditorSimulateModeHelper.SimulateBuild(AotConfig.PACKAGE_NAME);
                var packageRoot = buildResult.PackageRootDirectory;
                var editorFileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                var initParameters = new EditorSimulateModeParameters();
                initParameters.EditorFileSystemParameters = editorFileSystemParams;
                createParameters = initParameters;
            }
            // 单机运行模式
            else if (playMode == EPlayMode.OfflinePlayMode)
            {
                var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                var initParameters = new OfflinePlayModeParameters();
                initParameters.BuildinFileSystemParameters = buildinFileSystemParams;
                createParameters = initParameters;
            }
            // 联机运行模式
            else if (playMode == EPlayMode.HostPlayMode)
            {
                var remoteServices = new RemoteServices();
                var cacheFileSystemParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
                //var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                var initParameters = new HostPlayModeParameters();
                //initParameters.BuildinFileSystemParameters = buildinFileSystemParams;//这里根据游戏来确定，释放启用内置的文件查询，因为优化包体，内置可能不需要含资源
                initParameters.CacheFileSystemParameters = cacheFileSystemParams;
                createParameters = initParameters;
            }
            // WebGL运行模式
            else if (playMode == EPlayMode.WebPlayMode)
            {
                var remoteServices = new RemoteServices();
                var webServerFileSystemParams = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
                var webRemoteFileSystemParams = FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters(remoteServices); //支持跨域下载

                var initParameters = new WebPlayModeParameters();
                initParameters.WebServerFileSystemParameters = webServerFileSystemParams;
                initParameters.WebRemoteFileSystemParameters = webRemoteFileSystemParams;
                createParameters = initParameters;
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
        /*private class DecryptionServices : IDecryptionServices
        {
            public DecryptResult LoadAssetBundle(DecryptFileInfo fileInfo)
            {
                var res = new DecryptResult();
                res.Result = AssetBundle.LoadFromMemory(XIHDecryptionServices.Decrypt(File.ReadAllBytes(fileInfo.FileLoadPath)));
                return res;
            }

            public DecryptResult LoadAssetBundleAsync(DecryptFileInfo fileInfo)
            {
                var res = new DecryptResult();
                var managedStream = new MemoryStream(XIHDecryptionServices.Decrypt(File.ReadAllBytes(fileInfo.FileLoadPath)));
                res.CreateRequest = AssetBundle.LoadFromStreamAsync(managedStream);
                res.ManagedStream = managedStream;
                return res;
            }

            public byte[] ReadFileData(DecryptFileInfo fileInfo)
            {
                return XIHDecryptionServices.Decrypt(File.ReadAllBytes(fileInfo.FileLoadPath));
            }

            public string ReadFileText(DecryptFileInfo fileInfo)
            {
                return File.ReadAllText(fileInfo.FileLoadPath);
            }
        }*/
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

        //获取资源版本
        async UniTaskVoid UpdatePackageVersionAsync(ResourcePackage package)
        {
            var yooOp = package.RequestPackageVersionAsync();
            await yooOp.ToUniTask();
            if (yooOp.Status == EOperationStatus.Succeed)
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
            var yooOp = package.UpdatePackageManifestAsync(packageVersion,10);
            await yooOp.ToUniTask();
            if (yooOp.Status == EOperationStatus.Succeed)
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
            downloader.DownloadErrorCallback = OnDownloadError;
            downloader.DownloadUpdateCallback = OnDownloadProgress;

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
        void OnDownloadError(DownloadErrorData error)
        {
            Debug.LogError($"OnDownloadError:{error.FileName} > {error.ErrorInfo}");
        }
        void OnDownloadProgress(DownloadUpdateData updateData)
        {
            Debug.LogWarning($"正在下载({updateData.CurrentDownloadCount}/{updateData.TotalDownloadCount}): {(updateData.CurrentDownloadCount >> 10)}KB/{(updateData.TotalDownloadCount >> 10)}KB");
        }
    }
}
