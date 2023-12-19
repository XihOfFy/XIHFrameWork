
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using YooAsset;

namespace Aot
{
    //自定义启动logo界面
    public partial class AotMgr
    {
        public TMP_Text tip;
        bool isLogoEnd;
        private void StartLogo()
        {
            isLogoEnd = false;
            tip.text = "XIH\nGamer";
            tip.transform.DOScale(1, 2).From(0).OnComplete(() =>
            {
                isLogoEnd = true;
            });
        }
        private async UniTaskVoid EndLogo()
        {
            if (!isLogoEnd)
            {
                await UniTask.WaitUntil(() => isLogoEnd);
            }
            await YooAssets.LoadSceneAsync("Assets/Res/Aot2Hot/Scene/Aot2Hot.unity").ToUniTask();
        }
    }
}
