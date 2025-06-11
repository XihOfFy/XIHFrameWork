using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using Cysharp.Threading.Tasks;
using Aot;
namespace XiHUI
{
    public class EmitManager
    {
        static EmitManager _instance;
        public static EmitManager inst
        {
            get
            {
                if (_instance == null)
                    _instance = new EmitManager();
                return _instance;
            }
        }

        public string hurtFont1;
        public string hurtFont2;
        public string criticalSign;

        public GComponent view { get; private set; }

        private readonly Stack<EmitComponent> _componentPool = new Stack<EmitComponent>();
        readonly Vector2Int[] genDir = new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(-50, 50), new Vector2Int(50, 100), new Vector2Int(30, 60), new Vector2Int(-30, 60), new Vector2Int(-80, 80) };
        public EmitManager()
        {
            view = new GComponent();
            view.touchable = false;
            view.SetPivot(0.5f, 0.5f);
            GRoot.inst.AddChild(view);
        }
        public async UniTaskVoid EmitText(Vector3 worldPos, string txt, string fontUrl, int fontSize, int delay, int targetY = -128, float duration = 2f)
        {
            var screenPos = ChangeWorld2ScreenPos(worldPos);    //该方法必须在延迟前设置，避免玩家半路返回主界面，导致延迟后物体为空
            var localPos = ChangeScreen2LocalPos(screenPos);
            await UniTask.Delay(delay);
            var ec = GetEmitComponent();
            ec.EmitText(localPos, txt, fontUrl, fontSize, targetY, duration);
        }
        //世界坐标飞屏幕坐标
        public void EmitUrl(Vector3 worldPos, GObject fObj, string icon, int delay = 0, float toTargetDuration = 2f, float disappearDuration = 0.5f, bool rotate = false, System.Action OnPlaySoundCallback = null, int emitCount = 1, float scale = 1)
        {
            EmitUrl(ChangeWorld2ScreenPos(worldPos), fObj, icon, delay, toTargetDuration, disappearDuration, rotate, OnPlaySoundCallback, emitCount, scale);
        }
        // UI飞UI，使用屏幕坐标
        public void EmitUrl(Vector2 screenPos, GObject fObj, string icon, int delay = 0, float toTargetDuration = 2f, float disappearDuration = 0.5f, bool rotate = false, System.Action OnPlaySoundCallback = null, int emitCount = 1, float scale = 1)
        {
            var localPos = ChangeScreen2LocalPos(screenPos);
            EmitUrlInner(localPos, fObj, icon, delay, toTargetDuration, disappearDuration, rotate, OnPlaySoundCallback, emitCount, scale).Forget();
        }
        // UI飞UI，使用逻辑屏幕坐标
        public void EmitUI(Vector2 logicScreenPos, GObject fObj, string icon, int delay = 0, float toTargetDuration = 2f, float disappearDuration = 0.5f, bool rotate = false, System.Action OnPlaySoundCallback = null, int emitCount = 1, float scale = 1)
        {
            var localPos = ChangeLogicScreen2LocalPos(logicScreenPos);
            EmitUrlInner(localPos, fObj, icon, delay, toTargetDuration, disappearDuration, rotate, OnPlaySoundCallback, emitCount, scale).Forget();
        }
        async UniTaskVoid EmitUrlInner(Vector2 localPos, GObject fObj, string icon, int delay = 0, float toTargetDuration = 2f, float disappearDuration = 0.5f, bool rotate = false, System.Action OnPlaySoundCallback = null, int emitCount = 1, float scale = 1)
        {
            if (fObj == null || fObj.isDisposed)
            {
                Debug.LogError("飘向UI不存在");
                return;
            }
            var targetPos = fObj.TransformPoint(new Vector2(fObj.width, fObj.height) / 2, view);
            if (delay > 0) await UniTask.Delay(delay);
            var ec = GetEmitComponent();
            ec.EmitUrl(localPos, targetPos, icon, toTargetDuration, disappearDuration, rotate, OnPlaySoundCallback, scale);
            emitCount = Mathf.Min(emitCount, 10);
            foreach (var offset in genDir)
            {
                if (--emitCount <= 0) break;
                await UniTask.Delay(50);
                ec = GetEmitComponent();
                ec.EmitUrl(localPos + offset, targetPos, icon, toTargetDuration, disappearDuration, rotate, OnPlaySoundCallback, scale);
            }
        }

        public void EmitObj(string assetPath, Vector3 worldPos, GObject fObj, int delay = 0, float toTargetDuration = 2f, float disappearDuration = 0.5f)
        {
            var screenPos = ChangeWorld2ScreenPos(worldPos);    //该方法必须在延迟前设置，避免玩家半路返回主界面，导致延迟后物体为空
            var localPos = ChangeScreen2LocalPos(screenPos);
            EmitObjInner(assetPath, localPos, fObj, delay, toTargetDuration, disappearDuration).Forget();
        }
        public void EmitObjByLogicScreenPos(string assetPath, Vector2 logicScreenPos, GObject fObj, int delay = 0, float toTargetDuration = 2f, float disappearDuration = 0.5f)
        {
            var localPos = ChangeLogicScreen2LocalPos(logicScreenPos);
            EmitObjInner(assetPath, localPos, fObj, delay, toTargetDuration, disappearDuration).Forget();
        }
        async UniTaskVoid EmitObjInner(string assetPath, Vector2 localPos, GObject fObj, int delay = 0, float toTargetDuration = 2f, float disappearDuration = 0.5f)
        {
            var targetPos = fObj.TransformPoint(new Vector2(fObj.width, fObj.height) / 2, view);
            var prefab = AssetLoadUtil.LoadAssetAsync<GameObject>(assetPath);
            await prefab.ToUniTask();
            if (delay > 0) await UniTask.Delay(delay);
            var ec = GetEmitComponent();
            ec.EmitObj(prefab, localPos, targetPos, toTargetDuration, disappearDuration);
        }

        public void ReturnComponent(EmitComponent com)
        {
            _componentPool.Push(com);
        }
        //该方法必须在延迟前设置，避免玩家半路返回主界面，导致延迟后物体为空
        Vector2 ChangeWorld2ScreenPos(Vector3 worldPos)
        {
             Debug.LogError("待实现 ChangeWorld2ScreenPos");
             return Vector2.zero;
            /*var screenPos = GameBase.Instance.gameCamera.WorldToScreenPoint(worldPos);
            screenPos.y = Screen.height - screenPos.y; //convert to Stage coordinates system
            return screenPos;*/
        }

        //该方法必须在延迟前设置，避免玩家半路返回主界面，导致延迟后物体为空
        Vector2 ChangeScreen2LocalPos(Vector2 screenPos)
        {
            var localPos = view.GlobalToLocal(screenPos);
            return localPos;
        }
        Vector2 ChangeLogicScreen2LocalPos(Vector2 logicScreenPos)
        {
            var screenPos = GRoot.inst.LocalToGlobal(logicScreenPos);
            var localPos = ChangeScreen2LocalPos(screenPos);
            return localPos;
        }
        EmitComponent GetEmitComponent()
        {
            EmitComponent ec;
            if (_componentPool.Count > 0)
                ec = _componentPool.Pop();
            else
                ec = new EmitComponent();
            return ec;
        }
    }
}