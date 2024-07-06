using UnityEngine;
using FairyGUI;
using YooAsset;

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
            AddChild(_numberText);

            loader3D = new GLoader3D();
            loader3D.autoSize = true;
            loader3D.SetPivot(0.5f, 0.5f);
            AddChild(loader3D);
        }

        public void EmitText(Vector2 localPos, string txt, string fontUrl, int fontSize, int targetY = -128, float duration = 2f)
        {
            BeginLayout(localPos);
            SetText(txt, fontUrl, fontSize);
            EndLayout();
            var halfTime = duration * 0.5f;
            this.TweenMove(localPos + new Vector2(0, targetY), duration).OnComplete(this.OnCompleted);
            this.TweenFade(0, halfTime).SetDelay(halfTime);
        }

        public void EmitUrl(Vector2 localPos, Vector2 targetPos, string url, float toTargetDuration = 2f, float disappearDuration = 0.5f, bool rotate = true, System.Action OnPlaySoundCallback = null)
        {
            BeginLayout(localPos);
            SetUrl(url);
            EndLayout();
            DoRotete(rotate);
            this.TweenMove(targetPos, toTargetDuration).SetEase(EaseType.BackIn).OnComplete(() =>
            {
                if (OnPlaySoundCallback != null)
                {
                    OnPlaySoundCallback.Invoke();
                }
            });
            this.TweenFade(0, disappearDuration).SetDelay(toTargetDuration);
            this.TweenScale(Vector2.zero, disappearDuration).SetDelay(toTargetDuration).OnComplete(this.OnCompleted);
        }

        public void EmitObj(AssetHandle prefab, Vector2 localPos, Vector2 targetPos, float toTargetDuration = 2f, float disappearDuration = 0.5f)
        {
            BeginLayout(localPos);
            SetObj(prefab);
            EndLayout();
            this.TweenMove(targetPos, toTargetDuration).SetEase(EaseType.BackIn);
            this.TweenFade(0, disappearDuration).SetDelay(toTargetDuration);
            this.TweenScale(Vector2.zero, disappearDuration).SetDelay(toTargetDuration).OnComplete(() =>
            {
                prefab.Release();
                this.OnCompleted();
            });
        }
        void DoRotete(bool rotate)
        {
            if (!rotate) return;
            GTween.To(0, 180, 0.35f).SetTarget(this, TweenPropType.RotationY).SetEase(EaseType.Linear).SetRepeat(-1);
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