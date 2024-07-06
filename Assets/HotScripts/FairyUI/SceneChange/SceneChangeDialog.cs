using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;
using XiHUI;
using XiHUtil;
using YooAsset;
using FairyGUI;

namespace Hot
{
    public partial class SceneChangeDialog : UIDialog
    {
        static readonly Queue<Func<UniTask>> loadQue = new Queue<Func<UniTask>>();
        GTextField tip;

        /// <summary>
        /// 跳转场景，且完毕后直接打开对应界面
        /// </summary>
        /// <typeparam name="T">跳转后的界面</typeparam>
        /// <param name="path">场景路径</param>
        /// <param name="keepDialogs">界面不会因为切换场景而关闭</param>
        /// <returns></returns>
        public UniTask<T> Show<T>(string path, HashSet<string> keepDialogs = null) where T : UIDialog
        {
            var tcs = new UniTaskCompletionSource<T>();
            loadQue.Enqueue(() => {
                return Show(tcs, path, keepDialogs);
            });
            if (loadQue.Count > 1) return tcs.Task;//等待
            return Show(tcs, path, keepDialogs);
        }

        async UniTask<T> Show<T>(UniTaskCompletionSource<T> tcs, string path, HashSet<string> keepDialogs) where T : UIDialog
        {
            await _show(path, keepDialogs);
            var dialog = await UIUtil.OpenDialogAsync<T>();
            tcs.TrySetResult(dialog);
            OnLoadScene();
            return dialog;
        }

        /// <summary>
        /// 跳转场景
        /// </summary>
        /// <param name="path">场景路径</param>
        /// <param name="keepDialogs">界面不会因为切换场景而关闭</param>
        /// <returns></returns>
        public UniTask<SceneChangeDialog> Show(string path, HashSet<string> keepDialogs=null)
        {
            var tcs = new UniTaskCompletionSource<SceneChangeDialog>();
            loadQue.Enqueue(() => {
                return Show(tcs, path, keepDialogs);
            });
            if (loadQue.Count > 1) return tcs.Task;//等待
            return Show(tcs, path, keepDialogs);
        }
        async UniTask<SceneChangeDialog> Show(UniTaskCompletionSource<SceneChangeDialog> tcs, string path, HashSet<string> keepDialogs)
        {
            await _show(path, keepDialogs);
            tcs.TrySetResult(this);
            OnLoadScene();
            return this;
        }
        async UniTask _show(string path, HashSet<string> keepDialogs)
        {
            tip.SetText("正在加载场景:"+ path);
            await UniTask.Yield();//等待一帧，方便上一个UI展示完毕
            if (keepDialogs == null) keepDialogs = new HashSet<string>() { nameof(SceneChangeDialog) };
            else keepDialogs.Add(nameof(SceneChangeDialog));
            UIUtil.CloseAll(keepDialogs);//可以自己考虑加入哪些UI，跳转场景不会关闭的
            await UniTask.Yield();
            await YooAssets.LoadSceneAsync(path).ToUniTask();
        }
        void OnLoadScene()
        {
            loadQue.Dequeue();
            if (loadQue.Count > 0)
            {
                loadQue.Peek().Invoke().Forget();
                return;
            }
            Close();
            PlatformUtil.TriggerGC();
        }
    }
}
