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
        // 刻意以本类自有完成状态遮蔽基类 DownloadHandler.isDone（异步下载语义不同）
        public new bool isDone;
        
        private string _uri;
        private uint _crc;
        private AssetBundle _assetBundle;
        private MemoryStream _contentStream;
        private int _contentLength;
        // [V2 B.5] GetData() 的 ToArray 结果缓存，避免业务多次访问 .data 重复 ToArray 复制
        private byte[] _cachedData;
        
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
                    else if (_contentStream != null)
                    {
                        // [V2 B.5] 用 AssetBundle.LoadFromStream 替代 LoadFromMemory + ToArray()，
                        // 避免在 fallback 路径上把整包 AB 内容再复制一份到 LOH（峰值 ~2× AB 体积 → ~1×）。
                        // Unity 内部从 stream 直接读，无需 byte[] 拷贝。
                        _contentStream.Seek(0, SeekOrigin.Begin);
                        _assetBundle = AssetBundle.LoadFromStream(_contentStream, _crc);
                    }
                    else
                    {
                        _assetBundle = AssetBundle.LoadFromMemory(Array.Empty<byte>());
                    }
                }
                return _assetBundle;
            }
        }

        [Preserve]
        protected override byte[] GetData()
        {
            // [V2 B.5] 缓存 ToArray() 结果。业务代码（如 UnityWebRequest.downloadHandler.data）
            // 多次访问只触发一次复制，避免每次访问都生成 ~AB 体积大小的新 byte[]。
            if (_cachedData != null) return _cachedData;
            if (_contentStream == null) return null;
            _cachedData = _contentStream.ToArray();
            return _cachedData;
        }

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
