using Cysharp.Threading.Tasks;
using Hot;
using SimpleJSON;
using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace Tmpl
{
    public partial class Tables
    {
        public static Tables Instance { get; private set; }
        public static async UniTask LoadAllTmpl() {
            var asHandle = YooAssets.LoadAllAssetsAsync<TextAsset>("Assets/Res/Tmpl/tbuiparam.json");
            await asHandle.ToUniTask();
            var dic = new Dictionary<string, TextAsset>();
            foreach (var ast in asHandle.AllAssetObjects)
            {
                dic.Add(ast.name, ast as TextAsset);
            }
            Instance = new Tables((fn) => {
                return JSON.Parse(dic[fn].text);
            });
            asHandle.Release();
            await UniTask.Yield();//避免内存占用峰值高，GC一下
            PlatformUtil.TriggerGC();
        }
    }
}
