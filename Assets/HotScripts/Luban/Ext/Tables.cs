//#define JSON_LOAD
using Cysharp.Threading.Tasks;
using Luban;
using System.Collections.Generic;
using UnityEngine;
using Aot;

namespace Tmpl
{
    public partial class Tables
    {
        public static Tables Instance { get; private set; }
        public static async UniTask InitTmpl() {
#if JSON_LOAD
            var asHandle = AssetLoadUtil.LoadAllAssetsAsync<TextAsset>("Assets/Res/Tmpl/tbaudio.json");
#else
            var asHandle = AssetLoadUtil.LoadAllAssetsAsync<TextAsset>("Assets/Res/Tmpl/tbaudio.bytes");
#endif
            await asHandle.ToUniTask();
            var dic = new Dictionary<string, TextAsset>();
            var asts = asHandle.GetAssets<TextAsset>();
            foreach (var ast in asts)
            {
                dic.Add(ast.name, ast);
            }
            Instance = new Tables((fn) => {
#if JSON_LOAD
                return JSON.Parse(dic[fn].text);
#else
                return ByteBuf.Wrap(dic[fn].bytes);
#endif
            });
            
            asHandle.Release();
            AfterLoadTmpl();
        }

        static void AfterLoadTmpl() {

        }
#if UNITY_EDITOR
        public static void InitFromEditor() {
            Instance = new Tables((fn) => {
#if JSON_LOAD
                var asst = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/Res/Tmpl/{fn}.json");
                return JSON.Parse(asst.text);
#else
                var asst = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/Res/Tmpl/{fn}.bytes");
                return ByteBuf.Wrap(asst.bytes);
#endif
            });
            AfterLoadTmpl();
        }
#endif
    }
}
