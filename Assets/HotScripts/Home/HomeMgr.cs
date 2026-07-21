using Cysharp.Threading.Tasks;
using Tmpl;
using UnityEngine;
using XiHAsset;
using XiHUtil;
namespace Hot
{
    public partial class HomeMgr : XiHAssetBaseMgr<HomeMgr>
    {
        public HomeDialog homeDialog;
        public async UniTask ShowDialog()
        {
            homeDialog = await UIUtil.OpenDialogAsync<HomeDialog>();
            //homeDialog.Show();
        }
    }
}