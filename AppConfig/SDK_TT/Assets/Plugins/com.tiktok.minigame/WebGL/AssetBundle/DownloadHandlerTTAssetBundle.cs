using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;

namespace TTSDK
{
    /**
     * 只是用来承载 asset bundle 的获取，
     * TTAssetBundle.load 触发下载后，这里并不会获取到真正的 Asset Bundle 数据
     */
    public class DownloadHandlerTTAssetBundle : DownloadHandlerScript
    {
        public bool isDone;
        
        private string _uri;
        private uint _crc;
        private AssetBundle _assetBundle;
        private MemoryStream _contentStream;
        private int _contentLength;
        
        private static bool _isFallbackNoticed = false;
        
        public DownloadHandlerTTAssetBundle(string uri, uint crc)
        {
            _uri = uri;
            _crc = crc;
        }

        [Preserve]
        public AssetBundle assetBundle
        {
            get
            {
                if (_assetBundle == null)
                {
                    if (TTAssetBundle.isAbfsReady)
                    {
                        if (_contentLength != 0)
                        {
                            Debug.LogError($"DownloadHandlerTTAssetBundle contentLength not 0!");
                            return null;
                        }
                        _assetBundle = AssetBundle.LoadFromFile(_uri, _crc);
                        TTAssetBundle.bundle2path.Add(_assetBundle, _uri);
                    }
                    else
                    {
                        _assetBundle = AssetBundle.LoadFromMemory(_contentStream != null ? _contentStream.ToArray() : Array.Empty<byte>());
                    }
                }
                return _assetBundle;
            }
        }

        [Preserve]
        protected override byte[] GetData() => _contentStream != null ? _contentStream.ToArray() : null;

        [Preserve]
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || dataLength < 1)
                return false;
            
#if !(UNITY_WEBGL && !UNITY_EDITOR)
            if (!_isFallbackNoticed)
            {
                _isFallbackNoticed = true;
                Debug.LogWarning("TTAssetBundle 仅在 WebGL 方案有优化效果，当前环境下回滚到 UnityWebRequestAssetBundle 加载实现。");
            }
#endif
            
            if (_contentStream == null)
                _contentStream = new MemoryStream();
            _contentStream.Write(data, 0, dataLength);
            _contentLength += dataLength;
            return true;
        }

        [Preserve]
        protected override void CompleteContent() => isDone = true;
        
    }
}
