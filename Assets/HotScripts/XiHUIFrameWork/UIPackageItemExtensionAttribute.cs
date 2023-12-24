using System;

namespace XiHUI
{
    // 组件扩展时对应的组件类型
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class UIPackageItemExtensionAttribute : System.Attribute
    {
        public string url;
        public UIPackageItemExtensionAttribute(string url) { this.url = url; }
    }
}
