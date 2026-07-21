using Cysharp.Threading.Tasks;
using FairyGUI;
using FairyGUI.Utils;
using System;
using UnityEngine;
using XiHAsset;
using XiHUI;

namespace Hot
{
    public class GuideDialog : UIDialog
    {
        GuideMask mask;
        GuidHand hand;
        Action onClickAct;
        
        protected override void OnOpen()
        {
            GTween.Kill(hand);
            this.Content.onClick.Add(OnClickMask);
        }
        protected void OnClickMask()
        {
            var onClick = onClickAct;
            onClickAct = null;
            onClick?.Invoke();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="focusObj"></param>
        /// <param name="offset"></param>
        /// <param name="touchable">是否聚焦，只能点击该位置，一般true，则visable必须为true</param>
        /// <param name="visable"> 是否有黑色遮罩</param>
        public void Show(GObject focusObj, Vector2 offset,bool touchable = true, bool visable = true,Action onClick=null, float alpha = 0.6f) {
            Open();
            onClickAct = onClick;
            SetHandPos(focusObj, offset);
            mask.Render(focusObj, touchable, visable,alpha);
        }
        public void Show(Vector3 worldPos,Camera gamCamera, Vector2 offset) {
            Open();
            mask.SetVisible(false);
            hand.PlayAni(true);
            var screenPos = gamCamera.WorldToScreenPoint(worldPos);//UNITY左下原点，FGUI左上原点，但是stage camera却是中心为原点，且其已经偏移xy
            screenPos.y = Screen.height - screenPos.y;
            var pt = this.Content.GlobalToLocal(screenPos);
            pt += offset;
            hand.position = pt;
        }
        public void Show(Vector3 worldSourcePos,Vector3 worldTargetPos, Camera gamCamera, Vector2 offset)
        {
            Open();
            mask.SetVisible(false);
            hand.PlayAni(false);
            var screenPos = gamCamera.WorldToScreenPoint(worldSourcePos);
            screenPos.y = Screen.height - screenPos.y;
            var fromPos = this.Content.GlobalToLocal(screenPos);
            fromPos += offset;
            screenPos = gamCamera.WorldToScreenPoint(worldTargetPos);
            screenPos.y = Screen.height - screenPos.y;
            var toPos = this.Content.GlobalToLocal(screenPos);
            toPos += offset;
            hand.position = fromPos;
            hand.TweenMove(toPos, 2f).SetRepeat(-1);
        }
        void SetHandPos(GObject focus, Vector2 offset)
        {
            if (focus == null) return;
            hand.PlayAni(true);
            if (focus.displayObject == null)//Group是没有displayobjet的
            {
                //var pos = (Vector2)focus.position + focus.size / 2 + offset;
                var pos = (Vector2)focus.position + offset;
                hand.SetXY(pos.x, pos.y);
            }
            else
            {
                var pos = (Vector2)focus.GetRootPos() + offset;
                hand.SetXY(pos.x, pos.y);
            }
        }
    }
    [UIPackageItemExtension("ui://Guide/GuideMask")]
    class GuideMask : GComponent
    {
        GGraph bg;
        GGraph circle;
        public override void ConstructFromXML(XML xml)
        {
            UIControlBinding.BindFields(this, this);
        }

        public void Render(GObject focusObj, bool touchable,bool visable,float alpha)
        {
            this.touchable = touchable;
            this.SetVisible(visable);
            bg.alpha = alpha;
            if (focusObj == null)
            {
                circle.SetSize(0, 0);
                return;
            }
            Rect rect;
            if (focusObj.displayObject == null)//Group是没有displayobjet的
            {
                rect = new Rect(focusObj.position, focusObj.size);
            }
            else
            {
                rect = focusObj.TransformRect(new Rect(0, 0, focusObj.width, focusObj.height), this);
            }
            circle.SetSize(rect.size.x, rect.size.y);
            circle.SetPosition(rect.position.x, rect.position.y, circle.z);
            circle.SetSize(0, 0);
            circle.TweenResize(rect.size, 0.5f);
        }
        
        public void Render(bool touchable, bool visable, float alpha, Vector2 center,Vector2 size)
        {
            this.touchable = touchable;
            this.SetVisible(visable);
            bg.alpha = alpha;
            
            circle.SetSize(size.x, size.y);
            circle.SetPosition(center.x - size.x *0.5f, center.y - size.y *0.5f, circle.z);
            //circle.SetSize(0, 0);
            //circle.TweenResize(size, 0.5f);无法做动画，可能时间暂停了
        }
    }
    [UIPackageItemExtension("ui://Guide/GuidHand")]
    public class GuidHand : GComponent
    {
        GLoader3D handLoader;
        public override void ConstructFromXML(XML xml)
        {
            UIControlBinding.BindFields(this, this);
            //XiHAssetBaseMgr.BaseInstance.SetObj4GLoader3D(handLoader, "Assets/Res/Effect/Ani_FingerGuide/FingerGuide.prefab").Forget();
        }

        public void PlayAni(bool loop)
        {
            this.SetVisible(true);
        }

        public void CloseAni()
        {
            this.SetVisible(false);
        }
    }



    /*[UIPackageItemExtension("ui://Common/GuidHand")]
    public class GuidHand : GComponent
    {
        Transition ani;
        public override void ConstructFromXML(XML xml)
        {
            UIControlBinding.BindFields(this, this);
        }

        public void PlayAni(bool loop)
        {
            this.SetVisible(true);
            if (ani.playing) ani.Stop();
            ani.Play(loop?-1:1, 0,null);
        }

        public void CloseAni()
        {
            this.SetVisible(false);
            if (ani.playing) ani.Stop();
        }
    }*/
}
