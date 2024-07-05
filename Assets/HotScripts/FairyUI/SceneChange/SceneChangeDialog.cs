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
        /// ��ת����������Ϻ�ֱ�Ӵ򿪶�Ӧ����
        /// </summary>
        /// <typeparam name="T">��ת��Ľ���</typeparam>
        /// <param name="path">����·��</param>
        /// <param name="keepDialogs">���治����Ϊ�л��������ر�</param>
        /// <returns></returns>
        public UniTask<T> Show<T>(string path, HashSet<string> keepDialogs = null) where T : UIDialog
        {
            var tcs = new UniTaskCompletionSource<T>();
            loadQue.Enqueue(() => {
                return Show(tcs, path, keepDialogs);
            });
            if (loadQue.Count > 1) return tcs.Task;//�ȴ�
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
        /// ��ת����
        /// </summary>
        /// <param name="path">����·��</param>
        /// <param name="keepDialogs">���治����Ϊ�л��������ر�</param>
        /// <returns></returns>
        public UniTask<SceneChangeDialog> Show(string path, HashSet<string> keepDialogs=null)
        {
            var tcs = new UniTaskCompletionSource<SceneChangeDialog>();
            loadQue.Enqueue(() => {
                return Show(tcs, path, keepDialogs);
            });
            if (loadQue.Count > 1) return tcs.Task;//�ȴ�
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
            tip.SetText("���ڼ��س���:"+ path);
            await UniTask.Yield();//�ȴ�һ֡��������һ��UIչʾ���
            if (keepDialogs == null) keepDialogs = new HashSet<string>() { nameof(SceneChangeDialog) };
            else keepDialogs.Add(nameof(SceneChangeDialog));
            UIUtil.CloseAll(keepDialogs);//�����Լ����Ǽ�����ЩUI����ת��������رյ�
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
            PlatformUtil.TriggerGC().Forget();
        }
    }
}
