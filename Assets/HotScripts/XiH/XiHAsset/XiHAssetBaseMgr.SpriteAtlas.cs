using Aot.XiHUtil;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.U2D;

namespace XiHAsset
{
    public partial class XiHAssetBaseMgr
    {
        public const string ATLAS = "Atlas.spriteatlas";
        Dictionary<string, SpriteAtlas> spriteDic;//不建议使用Texture2D,因为图集可能就不只是这一个展示
        Dictionary<string, Texture2D> webTexDic;
        public async UniTask<Sprite> GetOneSpriteInAtlas(string path)
        {
            if (AssetLoadUtil.CheckLocationValid(path)) //优先本地找，不存在再考虑在图集里找
            {
                var handle = await GetHandle<Sprite>(path);
                return handle.GetAsset<Sprite>();
            }

            var fNmae = Path.GetFileNameWithoutExtension(path);
            var atlPath = path.Substring(0, path.LastIndexOf('/') + 1) + ATLAS;
            if (!spriteDic.ContainsKey(atlPath))
            {
                var handle = await GetHandle<SpriteAtlas>(atlPath);
                var atl = handle.GetAsset<SpriteAtlas>();
                spriteDic[atlPath] = atl;
            }
            return spriteDic[atlPath].GetSprite(fNmae);
        }
        //网路下载图片资源，若存在并发，可能有所内存溢出
        public async UniTask<Texture2D> GetTextureFromWeb(string url, int timeout = 10, CancellationToken cancelToken = default)
        {
            if (webTexDic.TryGetValue(url, out var texture2D))
            {
                if (texture2D) return texture2D;
            }
            try
            {
                var www = UnityWebRequestTexture.GetTexture(url);
                www.timeout = timeout;
                await www.SendWebRequest().ToUniTask(cancellationToken: cancelToken);
                if (www.result == UnityWebRequest.Result.Success)
                {
                    var texture = DownloadHandlerTexture.GetContent(www);
                    webTexDic[url] = texture;
                }
                www.Dispose();
            }
            catch
            {
                Debug.LogError("获取用户头像失败：" + url);
            }
            webTexDic.TryGetValue(url, out texture2D);
            return texture2D;
        }
    }
}
