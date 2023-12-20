using Aot;
using System.Collections;
using UnityEngine;
using YooAsset;

namespace Aot2Hot
{
    public partial class Aot2HotMgr
    {
        IEnumerator DownloadHotRes()
        {
            var package = YooAssets.GetPackage(AotConfig.PACKAGE_NAME);
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);

            //注册回调方法
            downloader.OnDownloadErrorCallback = OnDownloadError;
            downloader.OnDownloadProgressCallback = OnDownloadProgress;

            //没有需要下载的资源
            if (downloader.TotalDownloadCount == 0)
            {
                DownLoadEnd();
            }
            else {
                //开启下载
                downloader.BeginDownload();
                yield return downloader;
                //检测下载结果
                if (downloader.Status == EOperationStatus.Succeed)
                {
                    DownLoadEnd();
                }
                else
                {
                    TryReDownload();
                }
            }
        }

        void OnDownloadError(string fileName, string error) {
            Debug.LogError($"OnDownloadError:{fileName} > {error}");
        }
    }
}
