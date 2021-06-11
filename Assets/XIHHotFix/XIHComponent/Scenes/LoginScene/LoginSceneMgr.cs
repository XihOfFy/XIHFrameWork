using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using XIHBasic;
using XiHNet;

namespace XIHHotFix
{
    public class LoginSceneMgr : AbsComponent
    {
        private LoginSceneMgr(MonoDotBase dot) : base(dot) { }
        private GameObject loginUI;
        private GameObject enterUI;
        private InputField acc;
        private InputField pwd;
        private Text tips;
        private Button login;
        private Button register;
        private Button enter;
        NetAdapter loginClient;
        protected override void Awake()
        {
            loginUI = MonoDot.GameObjsDic["LoginUI"];
            enterUI = MonoDot.GameObjsDic["EnterUI"];
            acc = MonoDot.GameObjsDic["Acc"].GetComponent<InputField>();
            pwd = MonoDot.GameObjsDic["Pwd"].GetComponent<InputField>();
            tips = MonoDot.GameObjsDic["Tip"].GetComponent<Text>();
            login = MonoDot.GameObjsDic["Login"].GetComponent<Button>();
            register = MonoDot.GameObjsDic["Register"].GetComponent<Button>();
            enter = enterUI.GetComponent<Button>();

            login.onClick.AddListener(() =>
           {
                DoLogin();
           });
            register.onClick.AddListener(() =>
           {
               DoRegister();
           });
            enter.onClick.AddListener(() =>
           {
               ToLobby();
           });
            MonoNetMsgLooper.Instance.Clear();

            var address = IPAddress.Parse(IPConfig.CurCfg.loginIp);
            loginClient = new NetAdapter(IPConfig.CurCfg.netType, new IPEndPoint(address, IPConfig.CurCfg.loginPort));
            MonoNetMsgLooper.Instance.NetClients.TryAdd(NetServer.Login, loginClient);
        }
        protected override void OnDestory()
        {
            MonoNetMsgLooper.Instance.NetClients.TryRemove(NetServer.Login, out _);
        }

        protected override void OnDisable()
        {
        }

        protected override void OnEnable()
        {
        }
        private async void DoLogin()
        {

            //显示注册全部REQ RSP NTF 让ILR知道
            if (acc.text.Replace(" ", "") == "" || pwd.text.Replace(" ", "") == "")
            {
                tips.text = "请输入正确账号或密码";
                return;
            }
            //Debug.Log($"{acc.text}；{pwd.text}");
            if ((await loginClient.Request(new LoginReq() { Account = acc.text, LoginType = (int)LoginType.LoginByGm, Password = pwd.text })) is LoginRsp rsp)
            {
                if (rsp.Result == (int)LoginResultType.LoginResultSuccess)
                {
                    tips.text = "登录成功...正在进入大厅";
                    MonoNetMsgLooper.Instance.LoginRsp = rsp;
                    loginUI.SetActive(false);
                    enterUI.SetActive(true);
                }
                else
                {
                    tips.text = "登录失败，请检测账号密码是否正确...";
                }
            }
            else
            {
                tips.text = "正在进行连接登录服务器...";
                if (await loginClient.ConnectAsync())
                {
                    tips.text = "连接成功...即将登录";
                    DoLogin();
                }
                else
                {
                    tips.text = "连接登录服务器失败，请检测网络IP端口是否正确";
                }
            }
        }
        private async void DoRegister()
        {
            if (acc.text.Replace(" ", "") == "" || pwd.text.Replace(" ", "") == "")
            {
                tips.text = "请输入正确账号或密码";
                return;
            }
            if ((await loginClient.Request(new RegisterReq() { Account = acc.text, Password = pwd.text })) is RegisterRsp rsp)
            {
                if (rsp.Result)
                {
                    tips.text = "注册成功...可以执行登录";
                }
                else
                {
                    tips.text = "注册失败，请选用其他账号";
                }
            }
            else
            {
                tips.text = "正在进行连接登录服务器...";
                if (await loginClient.ConnectAsync())
                {
                    tips.text = "连接成功...即将注册";
                    DoRegister();
                }
                else
                {
                    tips.text = "连接登录服务器失败，请检测网络IP端口是否正确";
                }
            }
        }

        private async void ToLobby()
        {
            if (MonoNetMsgLooper.Instance.NetClients.ContainsKey(NetServer.Lobby))
            {
                NetAdapter reclient = MonoNetMsgLooper.Instance.NetClients[NetServer.Lobby];
                MonoNetMsgLooper.Instance.NetClients.TryRemove(NetServer.Lobby, out _);
                reclient.OnClosed = null;//禁止断线重连
                reclient.Close();
            }
            NetAdapter lobbyClient = new NetAdapter((NetworkProtocol)MonoNetMsgLooper.Instance.LoginRsp.NetProtocol, new IPEndPoint(IPAddress.Parse(MonoNetMsgLooper.Instance.LoginRsp.GateHost), MonoNetMsgLooper.Instance.LoginRsp.GatePort));
            lobbyClient.RegisterNtf<KickOutNtf>((ntf) =>
            {
                MonoNetMsgLooper.Instance.NetClients.TryRemove(NetServer.Lobby, out _);
                lobbyClient.Close();
            });
            lobbyClient.OnClosed = () => ReVerifyToLobbyAsync(lobbyClient, 3);
            if (await lobbyClient.ConnectAsync())
            {
                MonoNetMsgLooper.Instance.NetClients.TryAdd(NetServer.Lobby, lobbyClient);
                if ((await lobbyClient.Request(new LobbyVerifyReq() { SessionKey = MonoNetMsgLooper.Instance.LoginRsp.SessionKey })) is LobbyVerifyRsp lrsp)
                {
                    if (lrsp.Result)
                    {
                        MonoNetMsgLooper.Instance.VerifyRsp = lrsp;
                        lobbyClient.StartPingPong();
                        await Addressables.LoadSceneAsync("Assets/Bundles/Scenes/LobbyScene.unity").Task;
                        return;
                    }
                    else
                    {
                        tips.text = $"大厅服务器验证失败，请重新登录获取新的SessionKey";
                    }
                }
                else
                {
                    tips.text = $"连接异常，请重新连接";
                }
            }
            else
            {
                tips.text = $"连接大厅服务器失败，请检测该IP是否可以连接{MonoNetMsgLooper.Instance.LoginRsp.NetProtocol}=>{MonoNetMsgLooper.Instance.LoginRsp.GateHost}:{MonoNetMsgLooper.Instance.LoginRsp.GatePort}";
            }
            loginUI.SetActive(true);
            enterUI.SetActive(false);
        }
        //大厅断线重连
        async void ReVerifyToLobbyAsync(NetAdapter lobbyClient, int tryCount)
        {
            if (MonoNetMsgLooper.Instance.NetClients.ContainsKey(NetServer.Lobby))
            {
                if (--tryCount > 0)
                {
                    if (await lobbyClient.ConnectAsync())
                    {
                        if ((await lobbyClient.Request(new LobbyVerifyReq() { SessionKey = MonoNetMsgLooper.Instance.LoginRsp.SessionKey })) is LobbyVerifyRsp lrsp)
                        {
                            if (lrsp.Result)
                            {
                                MonoNetMsgLooper.Instance.VerifyRsp = lrsp;
                                lobbyClient.StartPingPong();
                                MonoNetMsgLooper.Instance.NotifyOnReVerify();
                            }
                            else
                            {
                                ToLogin("你被服务器踢出，请重新登录");
                            }
                            return;
                        }
                    }
                    await Task.Delay(NetConfig.RecTimeOut >> 2);//2.5s重试一次
                    ReVerifyToLobbyAsync(lobbyClient, tryCount);
                }
                else
                {
                    ToLogin("重连失败");
                }
            }
            else
            {
                ToLogin("你被服务器踢出，请重新登录1");
            }
        }
        async void ToLogin(string tip)
        {
            Debug.Log(tip);
            await Addressables.LoadSceneAsync("Assets/Bundles/Scenes/LoginScene.unity").Task;
        }
    }
}
