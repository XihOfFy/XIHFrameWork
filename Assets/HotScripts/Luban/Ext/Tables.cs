//#define JSON_LOAD
using Cysharp.Threading.Tasks;
using Hot;
using Luban;
using SimpleJSON;
using System;
using System.Collections.Generic;
using UnityEngine;
using XiHUtil;
using YooAsset;

namespace Tmpl
{
    public partial class Tables
    {
        public static Tables Instance { get; private set; }
        public static async UniTask LoadAllTmpl() {
#if JSON_LOAD
            var asHandle = YooAssets.LoadAllAssetsAsync<TextAsset>("Assets/Res/Tmpl/tbuiparam.json");
#else
            var asHandle = YooAssets.LoadAllAssetsAsync<TextAsset>("Assets/Res/Tmpl/tbuiparam.bytes");
#endif
            await asHandle.ToUniTask();
            var dic = new Dictionary<string, TextAsset>();
            foreach (var ast in asHandle.AllAssetObjects)
            {
                dic.Add(ast.name, ast as TextAsset);
            }
            Instance = new Tables((fn) => {
#if JSON_LOAD
                return JSON.Parse(dic[fn].text);
#else
                return ByteBuf.Wrap(dic[fn].bytes);
#endif
            });
            asHandle.Release();
            await UniTask.Yield();//避免内存占用峰值高，GC一下
            PlatformUtil.TriggerGC();
        }
    }
}
