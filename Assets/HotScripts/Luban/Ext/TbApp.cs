using Aot;
using Aot2Hot;
using UnityEngine;

namespace Tmpl
{
    public partial class TbApp
    {
        public static AppCfg AppCfg { get; private set; }
        public void AfterLoadTmpl()
        {
            AppCfg = Get(Aot2HotUtil.APPID);
        }
    }

}
