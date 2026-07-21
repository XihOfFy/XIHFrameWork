using Ad;
using Cysharp.Threading.Tasks;
using XiHUtil;
using UnityEngine.EventSystems;

namespace Hot
{
    public partial class HotMgr
    {
        async UniTask InitThridSdk()
        {
            var complete = false;
            ChannelSDKMgr.sdkBase.Init(res =>
            {
                complete = true;
            });
            AdManager.Create();
            await UniTask.WaitUntil(() => complete);
            ChannelSDKMgr.sdkBase.TouchOverride(this.gameObject);//处理小游戏平台触屏粘连，例如：摄像机射线监测点击物体，触发多次点击事件
            InputUtil.InitInputMoudle(GetComponent<StandaloneInputModule>());
            //DY SDK必须先初始化才能读取存档
            //但是internal渠道需要先读取存档，获取uuid
            DataSaveAgent.InitData();//当道SDK初始化后，初始化存档，那么尽量不要依赖配置表和其他初始化的数据
#if !USE_GM
            await DataSaveAgent.UploadRemoteData(true);
#endif
        }
    }
}

