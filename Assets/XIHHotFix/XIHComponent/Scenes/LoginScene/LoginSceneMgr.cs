using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using XIHBasic;
using XiHNet;

namespace XIHHotFix
{
    public class LoginSceneMgr : AbsComponent<MonoDotBase>
    {
        protected LoginSceneMgr(MonoDotBase dot) : base(dot) { }
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

            //??????????????????REQ RSP NTF ???ILR??????
            if (acc.text.Replace(" ", "") == "" || pwd.text.Replace(" ", "") == "")
            {
                tips.text = "??????????????????????????????";
                return;
            }
            //Debug.Log($"{acc.text}???{pwd.text}");
            if ((await loginClient.Request(new LoginReq() { Account = acc.text, LoginType = (int)LoginType.LoginByGm, Password = pwd.text })) is LoginRsp rsp)
            {
                if (rsp.Result == (int)LoginResultType.LoginResultSuccess)
                {
                    tips.text = "????????????...??????????????????";
                    MonoNetMsgLooper.Instance.LoginRsp = rsp;
                    loginUI.SetActive(false);
                    enterUI.SetActive(true);
                }
                else
                {
                    tips.text = "????????????????????????????????????????????????...";
                }
            }
            else
            {
                tips.text = "?????????????????????????????????...";
                if (await loginClient.ConnectAsync())
                {
                    tips.text = "????????????...????????????";
                    DoLogin();
                }
                else
                {
                    tips.text = "?????????????????????????????????????????????IP??????????????????";
                }
            }
        }
        private async void DoRegister()
        {
            if (acc.text.Replace(" ", "") == "" || pwd.text.Replace(" ", "") == "")
            {
                tips.text = "??????????????????????????????";
                return;
            }
            if ((await loginClient.Request(new RegisterReq() { Account = acc.text, Password = pwd.text })) is RegisterRsp rsp)
            {
                if (rsp.Result)
                {
                    tips.text = "????????????...??????????????????";
                }
                else
                {
                    tips.text = "????????????????????????????????????";
                }
            }
            else
            {
                tips.text = "?????????????????????????????????...";
                if (await loginClient.ConnectAsync())
                {
                    tips.text = "????????????...????????????";
                    DoRegister();
                }
                else
                {
                    tips.text = "?????????????????????????????????????????????IP??????????????????";
                }
            }
        }

        private async void ToLobby()
        {
            if (MonoNetMsgLooper.Instance.NetClients.ContainsKey(NetServer.Lobby))
            {
                NetAdapter reclient = MonoNetMsgLooper.Instance.NetClients[NetServer.Lobby];
                MonoNetMsgLooper.Instance.NetClients.TryRemove(NetServer.Lobby, out _);
                reclient.OnClosed = null;//??????????????????
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
                        await Addressables.LoadSceneAsync(PathConfig.AA_Scene_Lobby).Task;
                        return;
                    }
                    else
                    {
                        tips.text = $"?????????????????????????????????????????????????????????SessionKey";
                    }
                }
                else
                {
                    tips.text = $"??????????????????????????????";
                }
            }
            else
            {
                tips.text = $"??????????????????????????????????????????IP??????????????????{MonoNetMsgLooper.Instance.LoginRsp.NetProtocol}=>{MonoNetMsgLooper.Instance.LoginRsp.GateHost}:{MonoNetMsgLooper.Instance.LoginRsp.GatePort}";
            }
            loginUI.SetActive(true);
            enterUI.SetActive(false);
        }
        //??????????????????
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
                                ToLogin("???????????????????????????????????????");
                            }
                            return;
                        }
                    }
                    await Task.Delay(NetConfig.RecTimeOut >> 2);//2.5s????????????
                    ReVerifyToLobbyAsync(lobbyClient, tryCount);
                }
                else
                {
                    ToLogin("????????????");
                }
            }
            else
            {
                ToLogin("???????????????????????????????????????1");
            }
        }
        async void ToLogin(string tip)
        {
            Debug.Log(tip);
            await Addressables.LoadSceneAsync(PathConfig.AA_Scene_Login).Task;
        }
    }
}
