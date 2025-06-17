using Aot.XiHUtil;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;

namespace Aot
{
    //自定义启动logo界面
    public partial class AotMgr
    {
        public TMP_Text tip;
        bool isLogoEnd;
        private void StartLogo()
        {
            tip.text = "XIH\nGamer";
#if UNITY_EDITOR
            isLogoEnd = true;
#else
            isLogoEnd = false;
            tip.transform.DOScale(1, 2).From(0).OnComplete(() =>
            {
                isLogoEnd = true;
            });
#endif
        }
        private async UniTaskVoid EndLogo()
        {
            if (!isLogoEnd)
            {
                await UniTask.WaitUntil(() => isLogoEnd);
            }
            AssetLoadUtil.LoadScene("Assets/Res/Aot2Hot/Scene/Aot2Hot.unity").Forget();
        }
    }
}
