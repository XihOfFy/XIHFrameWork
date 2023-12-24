using FairyGUI;
using FairyGUI.Utils;
using UnityEngine;
using XiHUI;

namespace Hot
{
    //[UIPackageItemExtension("ui://4imy7ztwzplm9")]
    [UIPackageItemExtension("ui://Common/TipItem")]
    class TipItem : GComponent
    {
        public GTextField tip;
        public override void ConstructFromXML(XML xml)
        {
            UIControlBinding.BindFields(this, this);
        }
        public void Render(string tipStr)
        {
            tip.text = tipStr;
        }
    }
    public class SystemTipDialog : UIDialog
    {
        GList list;
        public void Show(string tipStr)
        {
            Debug.Log(tipStr);
        }
    }
}
