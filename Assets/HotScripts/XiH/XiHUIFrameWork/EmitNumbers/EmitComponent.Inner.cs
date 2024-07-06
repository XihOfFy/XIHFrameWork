using UnityEngine;
using FairyGUI;
using YooAsset;

namespace XiHUI
{
    public partial class EmitComponent
    {
        void BeginLayout(Vector2 localPos)
        {
            this.localPos = localPos;
            this.scale = Vector2.one;
            this.alpha = 1;
            this.rotationY = 0;
            _numberText.SetText("");
            _symbolLoader.SetUrl("");
            if (loader3D.wrapTarget != null) GameObject.Destroy(loader3D.wrapTarget);
        }
        void EndLayout()
        {
            _numberText.SetXY(-_numberText.width / 2, -_numberText.height / 2);
            _symbolLoader.SetXY(-_symbolLoader.width / 2, -_symbolLoader.height / 2);
            loader3D.SetXY(-loader3D.width / 2, -loader3D.height / 2);
            EmitManager.inst.view.AddChild(this);
            this.SetXY(localPos.x, localPos.y);
        }
        void SetText(string txt, string fontUrl, int fontSize)
        {
            var tf = _numberText.textFormat;
            tf.font = fontUrl;
            tf.size = fontSize;
            _numberText.textFormat = tf;
            _numberText.text = txt;
        }
        void SetUrl(string url)
        {
            _symbolLoader.url = url;
        }
        void SetObj(AssetHandle prefab)
        {
            if (loader3D.wrapTarget != null) GameObject.Destroy(loader3D.wrapTarget);
            loader3D.SetWrapTarget(prefab.InstantiateSync(), false, 1, 1);
        }
        void OnCompleted()
        {
            GTween.Kill(this);
            EmitManager.inst.view.RemoveChild(this);
            EmitManager.inst.ReturnComponent(this);
        }
    }
}