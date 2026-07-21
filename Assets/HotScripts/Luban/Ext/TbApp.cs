using Aot;

namespace Tmpl
{
    public partial class TbApp
    {
        public static AppCfg AppCfg { get; private set; }
        public void AfterLoadTmpl()
        {
            AppCfg = Get(AotConfig.APPID);
        }
    }
}
