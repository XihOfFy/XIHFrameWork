using Aot.XiHUtil;
using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using YooAsset;

namespace Aot
{
    public partial class AotMgr
    {
        public static string GetYooEditorSimulateManifestPath(string packageName)
        {
#if UNITY_EDITOR
            //若不存在，执行YooEditorSimulateModeHelperWindow类中的"YooAsset/YooEditorSimulateModeHelperBuild (耗时)"菜单栏
            return $"XIHWebServerRes/Bundles/{UnityEditor.EditorUserBuildSettings.activeBuildTarget}/{packageName}/Simulate";
#else 
            throw new FileNotFoundException("GetYooEditorSimulateManifestPath Simulate");
#endif
        }
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

                var packageRoot = GetYooEditorSimulateManifestPath(AotConfig.PACKAGE_NAME);
                var editorFileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                var initParameters = new EditorSimulateModeParameters();
                initParameters.EditorFileSystemParameters = editorFileSystemParams;
                createParameters = initParameters;
            }
            // 单机运行模式
            else if (playMode == EPlayMode.OfflinePlayMode)
            {
                //使用加密
                var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(new DecryptionServices());
                //var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                var initParameters = new OfflinePlayModeParameters();
                initParameters.BuildinFileSystemParameters = buildinFileSystemParams;
                createParameters = initParameters;
            }
            // 联机运行模式
            else if (playMode == EPlayMode.HostPlayMode)
            {
                //这里根据游戏来确定，释放启用内置的文件查询，因为优化包体，内置可能不需要含资源
                //var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                // 注意：设置参数COPY_BUILDIN_PACKAGE_MANIFEST，可以初始化的时候拷贝内置清单到沙盒目录
                //buildinFileSystemParams.AddParameter(FileSystemParametersDefine.COPY_BUILDIN_PACKAGE_MANIFEST, true);
                //使用加密
                var cacheFileSystemParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(new RemoteServices(), new DecryptionServices());
                // 注意：设置参数INSTALL_CLEAR_MODE，可以解决覆盖安装的时候将拷贝的内置清单文件清理的问题。
                //cacheFileSystemParams.AddParameter(FileSystemParametersDefine.INSTALL_CLEAR_MODE, EOverwriteInstallClearMode.None);
                var initParameters = new HostPlayModeParameters();
                //initParameters.BuildinFileSystemParameters = buildinFileSystemParams;
                initParameters.CacheFileSystemParameters = cacheFileSystemParams;
                createParameters = initParameters;
            }
            // WebGL运行模式
            else if (playMode == EPlayMode.WebPlayMode)
            {
                var remoteServices = new RemoteServices();
                //var webServerFileSystemParams = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
#if UNITY_WX
                //若是微信小游戏,cdn是defaultHostServer的前缀，且defaultHostServer的后缀/分隔的是微信缓存的文件夹路径且可多层级，保证名字固定
                //这样就得到packageRoot，到时候资源会缓存在packageRoot里面，后面执行 ClearCacheFilesAsync(EFileClearMode.ClearUnusedManifestFiles); 就能准确清理
                var cdn = AotConfig.frontConfig.cdn;
                var suffix = AotConfig.frontConfig.defaultHostServer.Substring(cdn.Length);
                if(suffix.StartsWith('/'))suffix = suffix.Substring(1);
                string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE/{suffix}";
                var webRemoteFileSystemParams = WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices);
#elif UNITY_DY //BYTEMINIGAME
                // 小游戏缓存根目录
                // 注意：如果有子目录，请修改此处！
                string packageRoot = $"";
                var webRemoteFileSystemParams = TiktokFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices);
#else
                var webRemoteFileSystemParams = FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters(remoteServices); //支持跨域下载
#endif

                var initParameters = new WebPlayModeParameters();
                initParameters.WebRemoteFileSystemParameters = webRemoteFileSystemParams;
                createParameters = initParameters;
            }

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
        //解密接口 针对非webgl平台和非小游戏native方案        
        //web 不支持ab加密，所以对于原始资源，我们自行加密，然后打ab
        private class DecryptionServices : IDecryptionServices
        {
            public DecryptResult LoadAssetBundle(DecryptFileInfo fileInfo)
            {
                var res = new DecryptResult();
                var offset = fileInfo.BundleName.ToLower().Sum(c => c);
                res.Result = AssetBundle.LoadFromFile(fileInfo.FileLoadPath, fileInfo.FileLoadCRC, (ulong)offset);
                return res;
            }

            public DecryptResult LoadAssetBundleAsync(DecryptFileInfo fileInfo)
            {
                var res = new DecryptResult();
                var offset = fileInfo.BundleName.ToLower().Sum(c => c);
                res.CreateRequest = AssetBundle.LoadFromFileAsync(fileInfo.FileLoadPath, fileInfo.FileLoadCRC, (ulong)offset);
                return res;
            }
            //原生文件不进行加密,或已经加密存储过
            public byte[] ReadFileData(DecryptFileInfo fileInfo)
            {
                return AotFileUtil.ReadFileBytes(fileInfo.FileLoadPath);
            }
            //原生文件不进行加密,或已经加密存储过
            public string ReadFileText(DecryptFileInfo fileInfo)
            {
                return AotFileUtil.ReadFile(fileInfo.FileLoadPath);
            }
        }
#if UNITY_EDITOR
        public class EncryptionServices : IEncryptionServices
        {
            public EncryptResult Encrypt(EncryptFileInfo fileInfo)
            {
                var result = new EncryptResult();
                if (fileInfo.BundleName.EndsWith(".rawfile"))
                {
                    result.Encrypted = false;//原生不加密,或已经加密存储过
                    //result.EncryptedData = File.ReadAllBytes(fileInfo.FilePath);不需要
                }
                else
                {
                    result.Encrypted = true;
                    var bytes = File.ReadAllBytes(fileInfo.FileLoadPath);
                    var offset = fileInfo.BundleName.ToLower().Sum(c => c);
                    var newBytes = new byte[bytes.Length + offset];
                    for (int i = 0; i < offset; ++i) newBytes[i] = (byte)((offset | i) % 0XF);
                    Array.Copy(bytes, 0, newBytes, offset, bytes.Length);
                    result.EncryptedData = newBytes;
                }
                return result;
            }
        }
#endif
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
            try {
                var yooOp = package.RequestPackageVersionAsync();
                await yooOp.ToUniTask();
                if (yooOp.Status == EOperationStatus.Succeed)
                {
                    //更新成功
                    UpdatePackageManifest(package, yooOp.PackageVersion).Forget();
                    return;
                }
            } catch (Exception e) {
                Debug.LogError(e);
            }

            //对于强制更新游戏，无法获取资源版本需要退出游戏
            //QuitGame();

            //对于单机游戏，可以在这里做差异化，使用本地的版本
            /* 若首包有资源，那么首次运行需要拷贝内置资源清单到沙盒内
            if (IsFirstRunApp)
            {
                // 注意：内置清单的版本需要开发者自己记录并填写
                // 注意：CopyBuildinManifestOperation类是YOO扩展示例的代码！
                var copyBuildinManifestOp = new CopyBuildinManifestOperation("DefaultPackage", "1.0");
                YooAssets.StartOperation(copyBuildinManifestOp);
                yield return copyBuildinManifestOp;
            }*/
            // 获取上次成功记录的版本
            string version = AotPlayerPrefsUtil.Get(AotPlayerPrefsUtil.GAME_RES_VERSION, string.Empty);
            //弱网环境尝试
            if (playMode == EPlayMode.HostPlayMode && !string.IsNullOrEmpty(version))
            {
                UpdatePackageManifest(package, version).Forget();
            }
            else
            {
                UpdatePackageVersionAsync(package).Forget();//无限尝试
            }
        }
        //UpdatePackageManifest
        async UniTaskVoid UpdatePackageManifest(ResourcePackage package,string packageVersion)
        {
            try
            {
                var yooOp = package.UpdatePackageManifestAsync(packageVersion, 10);
                await yooOp.ToUniTask();
                if (yooOp.Status == EOperationStatus.Succeed)
                {
                    //更新成功
                    DownloadAot2HotRes(package).Forget();
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            //弱网环境尝试
            if (playMode == EPlayMode.HostPlayMode)
            {
                DownloadAot2HotRes(package).Forget();
            }
            else
            {
                UpdatePackageManifest(package, packageVersion).Forget();//无限尝试
            }
        }
        //资源包下载,只下载aot2hot的tag资源，余下的到hot再继续下载，因为aot不提供ui功能
        async UniTaskVoid DownloadAot2HotRes(ResourcePackage package) {
            int downloadingMaxNum = 10;
            int failedTryAgain = 10;
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
