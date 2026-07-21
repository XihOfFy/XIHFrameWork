using UnityEngine;
using FairyGUI;
using XiHUtil;
namespace XiHUI
{
    public partial class EmitComponent : GComponent
    {
        GLoader _symbolLoader;
        GTextField _numberText;
        GLoader3D loader3D;
        Vector2 localPos;
        public EmitComponent()
        {
            this.touchable = false;

            this.SetPivot(0.5f, 0.5f);
            this.SetSize(0, 0);

            _symbolLoader = new GLoader();
            _symbolLoader.autoSize = true;
            _symbolLoader.SetPivot(0.5f, 0.5f);
            AddChild(_symbolLoader);

            _numberText = new GTextField();
            _numberText.autoSize = AutoSizeType.Both;
            _numberText.SetPivot(0.5f, 0.5f);
            _numberText.UBBEnabled = true;
            _numberText.align = AlignType.Center;
            AddChild(_numberText);

            loader3D = new GLoader3D();
            loader3D.autoSize = true;
            loader3D.SetPivot(0.5f, 0.5f);
            AddChild(loader3D);
        }

        public void EmitText(Vector2 localPos, string txt, string fontUrl, int fontSize, int targetY = -128, float duration = 2f)
        {
            BeginLayout(localPos, 1);
            SetText(txt, fontUrl, fontSize);
            EndLayout();
            var halfTime = duration * 0.5f;
            this.TweenMove(localPos + new Vector2(0, targetY), duration).OnComplete(this.OnCompleted);
            this.TweenFade(0, halfTime).SetDelay(halfTime);
        }

        public void EmitUrl(Vector2 localPos, Vector2 targetPos, string url, float toTargetDuration = 2f, float disappearDuration = 0.5f, bool rotate = true, System.Action OnPlaySoundCallback = null, float scale = 1)
        {
            BeginLayout(localPos, scale);
            SetUrl(url);
            EndLayout();
            DoRotate(rotate);

            /*this.TweenMove(targetPos, toTargetDuration).SetEase(EaseType.BackIn).OnComplete(() =>
            {
                if (OnPlaySoundCallback != null)
                {
                    OnPlaySoundCallback.Invoke();
                }
            });*/

           /* var dir = targetPos - localPos;
            var verDir = new Vector2(-dir.y, dir.x);
            var split = 3;
            var midDir = (dir + verDir)/ split;

            var controlPos1 = localPos - midDir;
            var controlPos2 = localPos  + dir / split * (split-2) - midDir;

            GTween.To(0f, 1f, toTargetDuration).SetEase(EaseType.CubicInOut).OnUpdate((GTweener tweener) =>
            {
                float t = tweener.value.x;
                var pos = CalculateBezierPoint(t, localPos, controlPos1, controlPos2, targetPos);
                this.SetXY(pos.x, pos.y);
            }).OnComplete(() => { OnPlaySoundCallback?.Invoke(); }).SetTarget(this);*/

            this.TweenMove(targetPos, toTargetDuration).SetEase(EaseType.BackIn).OnComplete(() => { OnPlaySoundCallback?.Invoke(); });
            this.TweenFade(0, disappearDuration).SetDelay(toTargetDuration);
            this.TweenScale(Vector2.zero, disappearDuration).SetDelay(toTargetDuration).OnComplete(this.OnCompleted);
        }

        public void EmitObj(AssetRef prefab, Vector2 localPos, Vector2 targetPos, float toTargetDuration = 2f, float disappearDuration = 0.5f, float scale = 1)
        {
            BeginLayout(localPos, scale);
            SetObj(prefab);
            EndLayout();
            //this.TweenMove(targetPos, toTargetDuration).SetEase(EaseType.Linear);

/*            var dir = targetPos - localPos;
            var verDir = new Vector2(-dir.y, dir.x);
            var split = 10;
            var midDir = (dir + verDir) / split;

            var controlPos1 = localPos + midDir;
            var controlPos2 = localPos + dir / split * (split - 2) + midDir;
            GTween.To(0f, 1f, toTargetDuration).SetDelay(0.65f).SetEase(EaseType.QuartIn).OnUpdate((GTweener tweener) =>
            {
                var pos = CalculateBezierPoint(tweener.value.x, localPos, controlPos1, controlPos2, targetPos);
                this.SetXY(pos.x, pos.y);
            }).SetTarget(this);
*/

            this.TweenMove(targetPos, toTargetDuration).SetEase(EaseType.BackIn);
            this.TweenFade(0, disappearDuration).SetDelay(toTargetDuration);
            this.TweenScale(Vector2.zero, disappearDuration).SetDelay(toTargetDuration).OnComplete(() =>
            {
                prefab.Release();
                this.OnCompleted();
            });
        }
        void DoRotate(bool rotate)
        {
            if (!rotate) return;
            GTween.To(0, 180, 0.35f).SetTarget(this, TweenPropType.RotationY).SetEase(EaseType.Linear).SetRepeat(-1);
        }
        Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector2 p = uuu * p0; // 第一项
            p += 3 * uu * t * p1; // 第二项
            p += 3 * u * tt * p2; // 第三项
            p += ttt * p3;        // 第四项

            return p;
        }
        public void Cancel()
        {
            if (this.parent != null)
            {
                GTween.Kill(this);
                EmitManager.inst.view.RemoveChild(this);
            }
            EmitManager.inst.ReturnComponent(this);
        }
    }
}