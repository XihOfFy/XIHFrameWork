using Aot.XiHUtil;
using Cysharp.Threading.Tasks;
using Hot;
using System.Collections.Generic;
using Tmpl;
using UnityEngine;
using XiHSound;
using XiHUI;

namespace XiHUtil
{
    public static partial class UIUtil
    {
        public async static UniTask<UIDialog> OpenDialogAsync(UIParamCfg param)
        {
            return await UIDialogManager.Instance.OpenAsync(param);
        }

        public async static UniTask<T> OpenDialogAsync<T>() where T : UIDialog
        {
            var key = typeof(T).Name;
            var parmas = Tables.Instance.TbUIParam.Get(key);
            return await OpenDialogAsync(parmas) as T;
        }
        public async static UniTask<T> LoadScene<T>(string path) where T : UIDialog
        {
            return await ((await OpenDialogAsync<SceneChangeDialog>()).Show<T>(path));
        }
        public static async UniTask LoadHomeScene()
        {
            var sceneDialog = await UIUtil.OpenDialogAsync<SceneChangeDialog>();
            await sceneDialog.Show<HomeDialog>("Assets/Res/HotScene/Home.unity");
        }
        // public async static UniTask LoadHomeSceneDirect()
        // {
        //     CloseAll(new HashSet<string>());
        //     await AssetLoadUtil.LoadScene("Assets/Res/HotScene/Home.unity");
        //     SoundMgr.Instance.PlayMainBGM();
        //     await HomeMgr.Instance.ShowDialog();
        // }
        // public async static UniTask LoadHomeScene()
        // {
        //     var startTime = Time.realtimeSinceStartup;
        //     var dialog = await (await OpenDialogAsync<SceneChangeDialog>()).Show("Assets/Res/HotScene/Home.unity", false);
        //     SoundMgr.Instance.PlayMainBGM();
        //     await HomeMgr.Instance.ShowDialog();
        //     await dialog.PlayOpen(startTime);
        // }
        // public async static UniTask LoadGameSceneDirect(StageCfg cfg)
        // {
        //     CloseAll(new HashSet<string>());
        //     await AssetLoadUtil.LoadScene("Assets/Res/HotScene/Game.unity");
        //     SoundMgr.Instance.PlayGameBGM(1);
        //     await Game1Mgr.Instance.InitGame(cfg);
        // }
        // public static bool TryLoadGameScene()
        // {
        //     if (!DataSave.Instance.GetStageCfg(out var stage)) return false;
        //     if (stage == null) return false;//循环关卡，不会出现为null
        //     LoadGameScene(stage).Forget();
        //     return true;
        // }
        // public async static UniTask LoadGameScene(StageCfg cfg)
        // {
        //     var startTime = Time.realtimeSinceStartup;
        //     var dialog = await (await OpenDialogAsync<SceneChangeDialog>()).Show("Assets/Res/HotScene/Game.unity", false);
        //     SoundMgr.Instance.PlayGameBGM(1);
        //     await Game1Mgr.Instance.InitGame(cfg);
        //     await dialog.PlayOpen(startTime);
        //     // if (cfg.TipNum > 0) UIUtil.ShowSystemTip(string.Format(3006.Translate(), cfg.TipNum));
        // }
    }
}
