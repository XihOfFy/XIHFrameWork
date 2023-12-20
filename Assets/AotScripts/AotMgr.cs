using Cysharp.Threading.Tasks;
using System.Linq;
using System.Reflection;
using UnityEngine;
using YooAsset;
using TMPro;
namespace Aot
{
    public partial class AotMgr : MonoBehaviour
    {
        EPlayMode playMode;
        private void Awake()
        {
            StartLogo();

#if UNITY_WEBGL
#if UNITY_EDITOR
            playMode = EPlayMode.EditorSimulateMode;
#else
            playMode = EPlayMode.WebPlayMode;
#endif
#else
            playMode = EPlayMode.HostPlayMode;
#endif

            if (EPlayMode.WebPlayMode == playMode || EPlayMode.HostPlayMode == playMode)
            {
                InitConfigStart(8).Forget();
            }
            else
            {//非联机模式直接跳到yooasset初始化
                InitYooAssetStart().Forget();
            }
        }

        async UniTaskVoid GotoAot2HotScene()
        {
            var rawOp = YooAssets.LoadAssetAsync<TextAsset>("Assets/Res/Aot2Hot/Raw/Aot2Hot.bytes");
            await rawOp.ToUniTask();
            if (rawOp.Status != EOperationStatus.Succeed)
            {
                QuitGame();
            }
#if !UNITY_EDITOR
            var hotUpdateAss =Assembly.Load(XIHDecryptionServices.Decrypt(((TextAsset)rawOp.AssetObject).bytes));
#else
            // Editor下无需加载，直接查找获得HotUpdate程序集
            var hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Aot2Hot");
#endif
            rawOp.Release();
            Debug.Log($"成功加载{hotUpdateAss.GetName()}热更程序集");
            EndLogo().Forget();
        }

        //AOT启动过程必须保持一切顺利，不然强制退出游戏，到了Hot才可以给予UI弹框选择，即AOT过程不要有任何UI提示
        void QuitGame()
        {
            Application.Quit();//尝试多次失败直接退出游戏，不让玩
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            Debug.LogError($"本地调试报错解决方法：找到Assets/Resources/{nameof(XIHFrontSetting)}.asset和参考{nameof(AotConfig.InitFrontConfig)}方法修改为你本地的web地址且删除项目根目录下的XIHWebServerRes/Front文件夹，然后重新运行程序自动生成它们；\n Windows下菜单栏 XIHUtil/Server/WebSvr 即可开启本地web服务；\n Mac用户请自行搭建web服务，且设置web根路径为 XIHWebServerRes (与Assets同层级)");
#endif
        }
    }
}
