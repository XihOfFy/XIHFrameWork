using FairyGUI;
using FairyGUI.Utils;
using System;
using XiHUI;

namespace Hot
{
    //[UIPackageItemExtension("ui://4imy7ztwzplmc")]
    [UIPackageItemExtension("ui://Common/ChooseContent")]
    class ChooseContent : GComponent{
        public GTextField tip;
        public override void ConstructFromXML(XML xml)
        {
            UIControlBinding.BindFields(this,this);
        }
        public void Render(string tipStr) {
            tip.text = tipStr;
        }
    }
    public class ChooseDialog : UIDialog
    {
        GTextField title;
        ChooseContent tip;
        GButton cancelBtn;
        GButton okBtn;
        public void Show(string titleStr, string tipStr,Action<bool> choose) {
            title.text = titleStr;
            tip.Render(tipStr);
            cancelBtn.onClick.Clear();
            okBtn.onClick.Clear();
            void ChooseAct(bool choosed) {
                Close();
                choose?.Invoke(choosed);
            }
            cancelBtn.onClick.Add(()=> ChooseAct(false));
            okBtn.onClick.Add(()=> ChooseAct(true));
        }

    }
}
