using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using XIHBasic;
using XiHNet;

namespace XIHHotFix
{
    public class LobbySceneMgr : AbsComponent<MonoDotBase>
    {
        protected LobbySceneMgr(MonoDotBase dot) : base(dot) { }
        NetAdapter lobbyClient;
        private InputField chatMsg;
        private Text chatInfos;
        private Button sendLobby;
        private Text selfName;
        private static string chatInfo = "";

        private GameObject lobbyUI;
        private Transform roomParent;
        private GameObject roomPrefab;
        private Button createRoom;
        private List<GameObject> rooms;

        private GameObject roomUI;
        private GameObject start;
        private Button startBtn;
        private Button leaveBtn;
        private Button sendRoom;
        private GameObject psParent;
        private Text[] players;

        private Button logoutBtn;


        private Action OnChatNtf;
        private Action<LobbyRoomJoinNtf> OnLobbyRoomJoinNtf;
        private Action<LobbyRoomLeaveNtf> OnLobbyRoomLeaveNtf;
        private Action<LobbyStartNtf> OnLobbyStartNtf;
        private async void StartBattle()
        {
            if (!await lobbyClient.Notify(new LobbyStartNtf()))
            {
                chatInfos.text += $"[Room] 发送 LobbyStartNtf 失败 \r\n";
            }
        }
        private async void ToBattle(LobbyStartNtf ntf)
        {
            if (MonoNetMsgLooper.Instance.NetClients.ContainsKey(NetServer.Battle))
            {
                NetAdapter reclient = MonoNetMsgLooper.Instance.NetClients[NetServer.Battle];
                MonoNetMsgLooper.Instance.NetClients.TryRemove(NetServer.Battle, out _);
                reclient.Close();
            }
            NetAdapter battleCli = new NetAdapter((NetworkProtocol)ntf.NetProtocol, new IPEndPoint(IPAddress.Parse(ntf.MapHost), ntf.MapPort));
            if (await battleCli.ConnectAsync())
            {
                if (MonoNetMsgLooper.Instance.LobbyJoinRoomRsp == null)
                {
                    battleCli.Close();
                    return;//防止房主点击开始，但是玩家却选择离开
                }
                MonoNetMsgLooper.Instance.NetClients.TryAdd(NetServer.Battle, battleCli);
                await Addressables.LoadSceneAsync(PathConfig.AA_Scene_Battle).Task;
            }
            else
            {
                chatInfos.text += $"[Room] 连接战斗服务器 {ntf.MapHost}: {ntf.MapPort} 失败 \r\n";
            }
        }
        public async void Chat(bool isRoom)
        {
            if (chatMsg.text == "") return;
            if (isRoom)
            {
                if (await lobbyClient.Notify(new LobbyChatRoomNtf() { Name = MonoNetMsgLooper.Instance.VerifyRsp.Name, Info = chatMsg.text }))
                    chatMsg.text = "";
            }
            else
            {
                if (await lobbyClient.Notify(new LobbyChatNtf() { Name = MonoNetMsgLooper.Instance.VerifyRsp.Name, Info = chatMsg.text }))
                {
                    chatMsg.text = "";
                }
            }
        }
        void OnReVerify()
        {
            lobbyClient = MonoNetMsgLooper.Instance.NetClients[NetServer.Lobby];
            chatInfos.text += $"[Lobby] 重新连接成功\r\n";
            selfName.text = $"名字: {MonoNetMsgLooper.Instance.VerifyRsp.Name}";
            LeaveRoom();
        }
        public async void CreateRoom()
        {
            if ((await lobbyClient.Request(new LobbyCreateRoomReq())) is LobbyCreateRoomRsp rsp)
            {
                if (rsp.Result)
                {
                    MonoNetMsgLooper.Instance.LobbyJoinRoomRsp = new LobbyJoinRoomRsp()
                    {
                        RoomId = rsp.RoomId,
                        IdxInRoom = 0,
                        OwnerIdxInRoom = 0,
                        PlayersName = new string[players.Length]
                    };
                    MonoNetMsgLooper.Instance.LobbyJoinRoomRsp.PlayersName[0] = MonoNetMsgLooper.Instance.VerifyRsp.Name;
                    SetRoomUI(MonoNetMsgLooper.Instance.LobbyJoinRoomRsp);
                }
                else
                {
                    chatInfos.text += $"[Lobby] 创建房间失败\r\n";
                }
            }
            else
            {
                chatInfos.text += $"[Lobby] LobbyCreateRoomReq 失败\r\n";
            }
        }
        public async void LeaveRoom()
        {
            if ((await lobbyClient.Request(new LobbyLeaveRoomReq())) is LobbyLeaveRoomRsp)
            {
                MonoNetMsgLooper.Instance.LobbyJoinRoomRsp = null;
                StartGetRooms();
            }
            else
            {
                chatInfos.text += $"[Room] LobbyLeaveRoomReq 失败\r\n";
            }
        }
        private CancellationTokenSource cts;
        async void StartGetRooms()
        {
            lobbyUI.SetActive(true);
            roomUI.SetActive(false);
            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();
            while (lobbyUI != null && lobbyUI.activeSelf)
            {
                if ((await lobbyClient.Request(new LobbyGetRoomsReq())) is LobbyGetRoomsRsp rsp)
                {
                    int count = rsp.Rooms == null ? 0 : rsp.Rooms.Length;
                    int size = rooms.Count;
                    for (int i = 0; i < count; ++i)
                    {
                        if (i < size)
                        {
                            rooms[i].SetActive(true);
                        }
                        else
                        {
                            var gb = GameObject.Instantiate(roomPrefab);
                            gb.transform.SetParent(roomParent);
                            gb.SetActive(true);
                            rooms.Add(gb);
                        }
                        SetClick(rooms[i].gameObject, rsp.Rooms[i]);
                    }
                    for (int i = count; i < size; ++i)
                    {
                        rooms[i].SetActive(false);
                    }
                }
                else
                {
                    chatInfos.text += $"[Lobby] LobbyGetRoomsReq 失败\r\n";
                }
                try { await Task.Delay(3000, cts.Token); }
                catch
                {
                    break;
                }
            }
        }
        private void SetClick(GameObject obj, RoomInfo info)
        {
            var btn = obj.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(async () =>
            {
                if ((await lobbyClient.Request(new LobbyJoinRoomReq() { RoomId = info.RoomId })) is LobbyJoinRoomRsp rspi)
                {
                    MonoNetMsgLooper.Instance.LobbyJoinRoomRsp = null;
                    if (rspi.IdxInRoom == -1)
                    {
                        chatInfos.text += $"[Lobby] 加入房间失败，房间已解散或在战斗ing \r\n";
                        return;
                    }
                    MonoNetMsgLooper.Instance.LobbyJoinRoomRsp = rspi;
                    SetRoomUI(rspi);
                }
                else
                {
                    chatInfos.text += $"[Lobby] LobbyJoinRoomReq 失败\r\n";
                }
            });
            obj.GetComponentInChildren<Text>().text = $"房间:{info.Name}";
        }
        private void SetRoomUI(LobbyJoinRoomRsp roomInfo)
        {
            lobbyUI.SetActive(false);
            roomUI.SetActive(true);
            start.SetActive(roomInfo.IdxInRoom == roomInfo.OwnerIdxInRoom);
            int len = roomInfo.PlayersName.Length;
            for (int j = 0; j < len; ++j)
            {
                if (string.IsNullOrEmpty(roomInfo.PlayersName[j]))
                {
                    players[j].text = $"虚座待席";
                }
                else
                {
                    players[j].text = $"{j}号玩家{(j == roomInfo.OwnerIdxInRoom ? "(房主)" : "")}\r\n{roomInfo.PlayersName[j]}";
                }
            }
        }
        public async void Logout()
        {
            await Addressables.LoadSceneAsync(PathConfig.AA_Scene_Login).Task;
        }

        protected override void Awake()
        {
            chatMsg = MonoDot.GameObjsDic["ChatMsgInput"].GetComponent<InputField>();
            chatInfos = MonoDot.GameObjsDic["ChatInfos"].GetComponent<Text>();
            sendLobby = MonoDot.GameObjsDic["SendLobby"].GetComponent<Button>();
            sendLobby.onClick.AddListener(() =>
            {
                Chat(false);
            });
            selfName = MonoDot.GameObjsDic["Name"].GetComponent<Text>();

            lobbyUI = MonoDot.GameObjsDic["LobbyUI"];
            roomParent = MonoDot.GameObjsDic["RoomParent"].transform;
            roomPrefab = MonoDot.GameObjsDic["RoomPrefab"];
            createRoom = MonoDot.GameObjsDic["CreateRoom"].GetComponent<Button>();
            createRoom.onClick.AddListener(() =>
            {
                CreateRoom();
            });
            roomUI = MonoDot.GameObjsDic["RoomUI"];
            start = MonoDot.GameObjsDic["Start"];
            startBtn = start.GetComponent<Button>();
            startBtn.onClick.AddListener(() =>
            {
                StartBattle();
            });
            leaveBtn = MonoDot.GameObjsDic["Leave"].GetComponent<Button>();
            leaveBtn.onClick.AddListener(() =>
            {
                LeaveRoom();
            });
            sendRoom = MonoDot.GameObjsDic["SendRoom"].GetComponent<Button>();
            sendRoom.onClick.AddListener(() =>
            {
                Chat(true);
            });
            psParent = MonoDot.GameObjsDic["PsParent"];
            players = psParent.GetComponentsInChildren<Text>();

            logoutBtn= MonoDot.GameObjsDic["Logout"].GetComponent<Button>();
            logoutBtn.onClick.AddListener(() =>
            {
                Logout();
            });
        }

        protected override void OnEnable()
        {
            MonoNetMsgLooper.Instance.OnReVerify += OnReVerify;
            selfName.text = $"名字: {MonoNetMsgLooper.Instance.VerifyRsp.Name}";
            rooms = new List<GameObject>();
            lobbyClient = MonoNetMsgLooper.Instance.NetClients[NetServer.Lobby];
            void SaveChatInfo()
            {
                if (chatInfo.Length > 2048)
                {
                    chatInfo = chatInfo.Remove(0, chatInfos.text.Length - 1024);
                }
                chatInfos.text = chatInfo;
            }
            OnChatNtf = SaveChatInfo;
            lobbyClient.RegisterNtf<LobbyChatNtf>((ntf) =>
            {
                chatInfo += $"[Lobby] {ntf.Name}: {ntf.Info}\r\n";
                OnChatNtf?.Invoke();
            });
            lobbyClient.RegisterNtf<LobbyChatRoomNtf>((ntf) =>
            {
                chatInfo += $"[Room] {ntf.Name}: {ntf.Info}\r\n";
                OnChatNtf?.Invoke();
            });
            OnLobbyRoomJoinNtf = ntf =>
            {
                players[ntf.OrderInRoom].text = $"{ntf.OrderInRoom}号玩家\r\n{ntf.PlayerName}";
            };
            lobbyClient.RegisterNtf<LobbyRoomJoinNtf>(ntf =>
            {
                MonoNetMsgLooper.Instance.LobbyJoinRoomRsp.PlayersName[ntf.OrderInRoom] = ntf.PlayerName;
                OnLobbyRoomJoinNtf?.Invoke(ntf);
            });
            OnLobbyRoomLeaveNtf = ntf =>
            {
                players[ntf.OrderInRoom].text = $"虚座待席";
                if (!players[ntf.OwnerIdxInRoom].text.Contains("房主"))
                {
                    string ownerName = players[ntf.OwnerIdxInRoom].text.TrimStart($"{ntf.OwnerIdxInRoom}号玩家\r\n".ToCharArray());
                    players[ntf.OwnerIdxInRoom].text = $"{ntf.OwnerIdxInRoom}号玩家(房主)\r\n{ownerName}";
                }
                start.SetActive(ntf.OwnerIdxInRoom == MonoNetMsgLooper.Instance.LobbyJoinRoomRsp.IdxInRoom);
            };
            lobbyClient.RegisterNtf<LobbyRoomLeaveNtf>(ntf =>
            {
                MonoNetMsgLooper.Instance.LobbyJoinRoomRsp.OwnerIdxInRoom = ntf.OwnerIdxInRoom;
                MonoNetMsgLooper.Instance.LobbyJoinRoomRsp.PlayersName[ntf.OrderInRoom] = null;
                OnLobbyRoomLeaveNtf?.Invoke(ntf);
            });
            OnLobbyStartNtf = (ntf) =>
            {
                ToBattle(ntf);
            };
            lobbyClient.RegisterNtf<LobbyStartNtf>((ntf) => OnLobbyStartNtf?.Invoke(ntf));
            SaveChatInfo();
            var roomInfo = MonoNetMsgLooper.Instance.LobbyJoinRoomRsp;
            if (roomInfo == null)
            {
                StartGetRooms();
            }
            else
            {
                SetRoomUI(roomInfo);
            }
        }

        protected override void OnDisable()
        {
            MonoNetMsgLooper.Instance.OnReVerify -= OnReVerify;
            OnChatNtf = null;
            OnLobbyRoomJoinNtf = null;
            OnLobbyRoomLeaveNtf = null;
            //OnLobbyStartNtf = null;
        }

        protected override void OnDestory()
        {
        }
    }
}
