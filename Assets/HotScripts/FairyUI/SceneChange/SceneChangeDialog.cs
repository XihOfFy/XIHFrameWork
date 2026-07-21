using Cysharp.Threading.Tasks;
using FairyGUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using XiHUI;
using XiHUtil;
using XiHAsset;
using Aot.XiHUtil;

namespace Hot
{
    //切换场景专用
    public class SceneChangeDialog : UIDialog
    {
        Queue<Func<UniTask>> loadQue;
        Transition playAni;
        Transition playBackAni;
        //GTextField title;
        protected override void InitComponent()
        {
            loadQue = new Queue<Func<UniTask>>();
            //title.Translate(780001);
        }
        public UniTask<T> Show<T>(string path) where T : UIDialog
        {
            var tcs = new UniTaskCompletionSource<T>();
            loadQue.Enqueue(() =>
            {
                return Show(tcs, path);
            });
            if (loadQue.Count > 1) return tcs.Task;//等待
            return Show(tcs, path);
        }
        async UniTask<T> Show<T>(UniTaskCompletionSource<T> tcs, string path) where T : UIDialog
        {
            await _show(path);
            var dialog = await UIUtil.OpenDialogAsync<T>();
            tcs.TrySetResult(dialog);
            OnLoadScene();
            return dialog;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="autoOpen"> true，和一般无异，false，需要自己调用PlayOpen</param>
        /// <returns></returns>
        public UniTask<SceneChangeDialog> Show(string path, bool autoOpen = true)
        {
            var tcs = new UniTaskCompletionSource<SceneChangeDialog>();
            loadQue.Enqueue(() =>
            {
                return Show(tcs, path, autoOpen);
            });
            if (loadQue.Count > 1) return tcs.Task;//等待
            return Show(tcs, path, autoOpen);
        }
        async UniTask<SceneChangeDialog> Show(UniTaskCompletionSource<SceneChangeDialog> tcs, string path, bool autoOpen = true)
        {
            await _show(path);
            tcs.TrySetResult(this);
            if (autoOpen)
            {
                OnLoadScene();
            }
            return this;
        }
        public async UniTask PlayOpen(float startTime)
        {

            if (this == null || this.State != State.Open)
            {
                Debug.LogError($"切换场景UI已经关闭或者消失了，无需执行");
            }
            else
            {
                OnLoadScene();
                await UniTask.WaitUntil(() => this.State != State.Open);
            }
        }
        async UniTask _show(string path)
        {
            playAni.Play();
            var endTime = playAni.totalDuration + Time.realtimeSinceStartup;
            await UniTask.Delay(400);//等待界面显示覆盖，就可以提前开始进行后台操作
            UIUtil.CloseAll(new HashSet<string>() { nameof(SceneChangeDialog) });
            await UniTask.Yield();
            AssetPoolMgr.Instance.Recycle();
            await AssetLoadUtil.LoadScene(path);
            PlatformUtil.TriggerGC();
            var dur = Time.realtimeSinceStartup;
            var waitTime = (int)((endTime - Time.realtimeSinceStartup) * 1000);
            if (waitTime > 50) await UniTask.Delay(waitTime);
        }
        void OnLoadScene()
        {
            loadQue.Dequeue();
            if (loadQue.Count > 0)
            {
                loadQue.Peek().Invoke().Forget();
                return;
            }
            playBackAni.Play(Close);
            PlatformUtil.TriggerGC();
        }
    }
}
