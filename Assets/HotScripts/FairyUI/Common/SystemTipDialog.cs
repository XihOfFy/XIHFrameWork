using FairyGUI;
using FairyGUI.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using XiHUI;

namespace Hot
{
    //[UIPackageItemExtension("ui://4imy7ztwzplm9")]
    [UIPackageItemExtension("ui://Common/TipItem")]
    public class TipItem : GComponent
    {
        GTextField tip;
        Transition show;
        public override void ConstructFromXML(XML xml)
        {
            UIControlBinding.BindFields(this, this);
        }
        public void Render(string tipStr, Action<TipItem> callBack)
        {
            Center();
            this.SetVisible(true);
            tip.text = tipStr;
            //不使用动效回调是因为有时候topmost stack同层级多个UI出现，会阻碍动效播放回调，以至于无法继续后续逻辑MoveStage
            show.Play(() =>
            {
                callBack(this);
            });
        }
    }
    public class SystemTipDialog : UIDialog
    {
        Stack<TipItem> stackPool;
        protected override void InitComponent()
        {
            stackPool = new Stack<TipItem>();
            this.Content.touchable = false;
        }
        public void Show(string tipStr)
        {
#if UNITY_EDITOR
            Debug.Log(tipStr);
#endif
            var item = GetFromPool();
            this.Content.SetChildIndex(item, this.Content.numChildren - 1);
            item.Render(tipStr, Return2Pool);
        }
        TipItem GetFromPool()
        {
            if (stackPool.Count > 0) return stackPool.Pop();
            GObject obj = UIPackage.CreateObjectFromURL("ui://Common/TipItem");
            this.Content.AddChild(obj);
            return (TipItem)obj;
        }
        public void Return2Pool(TipItem obj)
        {
            obj.SetVisible(false);
            stackPool.Push(obj);
        }
    }
}
