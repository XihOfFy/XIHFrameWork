using FairyGUI;
using FairyGUI.Utils;
using System;
using UnityEngine;
using XiHUI;

namespace Hot
{
    //[UIPackageItemExtension("ui://4imy7ztwzplm9")]
    [UIPackageItemExtension("ui://Common/TipItem")]
    class TipItem : GComponent
    {
        GTextField tip;
        Transition show;
        public override void ConstructFromXML(XML xml)
        {
            UIControlBinding.BindFields(this, this);
        }
        public void Render(string tipStr,Action<TipItem> endPlay)
        {
            tip.text = tipStr;
            show.Play(()=> endPlay?.Invoke(this));
        }
    }
    public class SystemTipDialog : UIDialog
    {
        GList list;
        public void Show(string tipStr)
        {
            var item = list.AddItemFromPool() as TipItem;
            item.Render(tipStr,it=> {
                list.RemoveChildToPool(it);
            });
        }
    }
}
