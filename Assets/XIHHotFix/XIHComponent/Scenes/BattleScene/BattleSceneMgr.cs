using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XIHBasic;
using XiHNet;

namespace XIHHotFix
{
    public class BattleSceneMgr : AbsComponent<MonoTouch>
    {
        protected BattleSceneMgr(MonoTouch dot) : base(dot) { }
        NetAdapter battleClient;
        public GameObject playerPrefab;
        public Action<BattleStartNtf> battleStartNtf;
        public Action<PositionNtf> postionAct;
        private Action<LobbyRoomLeaveNtf> OnLobbyRoomLeaveNtf;

        public Action battleEndNtf;
        private Dictionary<int, Robot> robots;
        private Robot selfRb;
        private Rigidbody selfRgb;
        public Transform followCam;
        private Vector3 deltaCam;
        public RectTransform moveImg;
        public Text cdTimeTx;
        private Button exitBtn;
        private int dir = 0;
        private Dictionary<int, Vector3> dirSpeed;
        protected override void Awake()
        {
            MonoDot.onBeginDrag = OnBeginDrag;
            MonoDot.onDrag = OnDrag;
            MonoDot.onEndDrag = OnEndDrag;
            playerPrefab = MonoDot.GameObjsDic["Player"];
            followCam = MonoDot.GameObjsDic["PlayerCamera"].transform;
            moveImg = MonoDot.GameObjsDic["Point"].GetComponent<RectTransform>();
            cdTimeTx = MonoDot.GameObjsDic["CD"].GetComponent<Text>();
            exitBtn = MonoDot.GameObjsDic["Exit"].GetComponent<Button>();
            exitBtn.onClick.AddListener(async () =>
            {
                var lobbyClient = MonoNetMsgLooper.Instance.NetClients[NetServer.Lobby];
                if ((await lobbyClient.Request(new LobbyLeaveRoomReq())) is LobbyLeaveRoomRsp)
                {
                    OnReVerify();
                }
                else
                {
                    Debug.Log("离开失败");
                }
            });
        }

        protected async override void OnEnable()
        {
            HotFixInit.Update += Update;
            HotFixInit.FixedUpdate += FixedUpdate;
            deltaCam = followCam.position;
            dirSpeed = new Dictionary<int, Vector3>();
            dirSpeed.Add(0, Vector3.zero);
            int speed = 8;
            dirSpeed.Add(1, Vector3.forward * Time.fixedDeltaTime * speed);
            dirSpeed.Add(2, Vector3.back * Time.fixedDeltaTime * speed);
            dirSpeed.Add(3, Vector3.left * Time.fixedDeltaTime * speed);
            dirSpeed.Add(4, Vector3.right * Time.fixedDeltaTime * speed);
            robots = new Dictionary<int, Robot>();
            MonoNetMsgLooper.Instance.OnReVerify += OnReVerify;
            var lobbyClient = MonoNetMsgLooper.Instance.NetClients[NetServer.Lobby];
            OnLobbyRoomLeaveNtf = ntf =>
            {
                if (robots.ContainsKey(ntf.OrderInRoom))
                {
                    var rm = robots[ntf.OrderInRoom];
                    GameObject.Destroy(rm.playerTr.gameObject);
                    robots.Remove(ntf.OrderInRoom);
                }
            };
            lobbyClient.RegisterNtf<LobbyRoomLeaveNtf>(ntf =>
            {
                MonoNetMsgLooper.Instance.LobbyJoinRoomRsp.OwnerIdxInRoom = ntf.OwnerIdxInRoom;
                MonoNetMsgLooper.Instance.LobbyJoinRoomRsp.PlayersName[ntf.OrderInRoom] = null;
                OnLobbyRoomLeaveNtf?.Invoke(ntf);
            });
            battleClient = MonoNetMsgLooper.Instance.NetClients[NetServer.Battle];
            battleStartNtf = async (ntf) =>
            {
                battleStartNtf = null;
                robots.Clear();
                int max = 0;
                foreach (var r in ntf.Robots)
                {
                    Robot rb = new Robot
                    {
                        orderInRoom = r.OrderInRoom,
                        isSelf = r.OrderInRoom == MonoNetMsgLooper.Instance.LobbyJoinRoomRsp.IdxInRoom
                    };
                    var gb = GameObject.Instantiate(playerPrefab);
                    gb.SetActive(true);
                    gb.name = r.Name;
                    rb.name = gb.GetComponentInChildren<TextMesh>();
                    rb.name.text = r.Name;
                    rb.playerTr = gb.transform;
                    rb.lastVec3 = new Vector3(r.OrderInRoom * 1.0f / 2.0f, 0, r.OrderInRoom * 1.0f / 2.0f);
                    rb.playerTr.position = rb.lastVec3;
                    rb.targetVec3 = rb.lastVec3;
                    rb.playerTr.GetComponent<Collider>().isTrigger = !rb.isSelf;
                    rb.name.color = rb.isSelf ? Color.blue : Color.red;
                    robots.Add(r.OrderInRoom, rb);
                    if (rb.isSelf)
                    {
                        selfRb = rb;
                        var rg = rb.playerTr.gameObject.AddComponent<Rigidbody>();
                        rg.constraints = RigidbodyConstraints.FreezePositionY;
                        rg.freezeRotation = true;
                        rg.useGravity = false;
                        selfRgb = rg;
                    }
                    max = Math.Max(r.OrderInRoom, max);
                }
                refV3others = new Vector3[max + 1];
                var desT = DateTime.Now.AddMilliseconds(ntf.CDTime);
                int cdTime = (desT - DateTime.Now).Seconds;
                while (cdTime >= 0)
                {
                    if (cdTimeTx == null)
                    {
                        break;
                    }
                    else
                    {
                        cdTimeTx.text = $"CD:{cdTime}";
                    }
                    await Task.Delay(1000);
                    cdTime = (desT - DateTime.Now).Seconds;
                }
				if (cdTimeTx != null)
                    cdTimeTx.text = "CD:0";
            };
            battleClient.RegisterNtf<BattleStartNtf>((ntf) =>
            {
                battleStartNtf?.Invoke(ntf);
            });
            battleEndNtf = async() =>
            {
                MonoNetMsgLooper.Instance.NetClients.TryRemove(NetServer.Battle, out _);
                battleClient.Close();
                await Addressables.LoadSceneAsync("Assets/Bundles/Scenes/LobbyScene.unity").Task;
            };
            battleClient.RegisterNtf<BattleEndNtf>((ntf) =>
            {
                battleEndNtf?.Invoke();
            });
            postionAct = (ntf) =>
            {
                if (robots.ContainsKey(ntf.PlayerOrderInRoom))
                {
                    var rb = robots[ntf.PlayerOrderInRoom];
                    rb.targetVec3 = new Vector3(ntf.X, 0, ntf.Z);
                }
            };
            battleClient.RegisterNtf<PositionNtf>((ntf) =>
            {
                postionAct?.Invoke(ntf);
            });
            await battleClient.Notify(new BattleReadyNtf()
            {
                PlayerOrderInRoom = MonoNetMsgLooper.Instance.LobbyJoinRoomRsp.IdxInRoom,
                RoomId = MonoNetMsgLooper.Instance.LobbyJoinRoomRsp.RoomId,
                SessionKey = MonoNetMsgLooper.Instance.LoginRsp.SessionKey,
            });
            battleClient.StartPingPong();
			battleClient.OnClosed = () => {
                if (MonoNetMsgLooper.Instance.NetClients.ContainsKey(NetServer.Battle)) {
                    //切换后台过久导致断线
                    OnReVerify();
                }
            };
        }
        short frame = 0;
        Vector3[] refV3others;
        private void FixedUpdate()
        {
            foreach (var rb in robots.Values)
            {
                if (rb.isSelf) continue;
                //rb.playerTr.position = Vector3.Lerp(rb.lastVec3, rb.targetVec3, 0.1f);
                rb.playerTr.position = Vector3.SmoothDamp(rb.lastVec3, rb.targetVec3, ref refV3others[rb.orderInRoom], 0.25f);
                rb.lastVec3 = rb.playerTr.position;
            }
            if (selfRgb == null) return;
            ++frame;
            if (frame >= 5)
            {
                frame = 0;
                SendSelfPos();
            }
            //selfRb.playerTr.Translate(dirSpeed[dir], Space.World);
            selfRgb.MovePosition(Vector3.SmoothDamp(selfRgb.position, selfRgb.position + dirSpeed[dir], ref refV3others[selfRb.orderInRoom], 0.03125f));
            followCam.position = Vector3.Lerp(followCam.position, selfRgb.position + deltaCam, 0.25f);
        }

        private void Update()
        {
#if UNITY_EDITOR && UNITY_EDITOR_WIN
            if (Keyboard.current.leftShiftKey.isPressed)
            {
                if (Keyboard.current.upArrowKey.isPressed)
                {
                    dir = 1;
                }
                else if (Keyboard.current.downArrowKey.isPressed)
                {
                    dir = 2;
                }
                else if (Keyboard.current.leftArrowKey.isPressed)
                {
                    dir = 3;
                }
                else if (Keyboard.current.rightArrowKey.isPressed)
                {
                    dir = 4;
                }
                else
                {
                    dir = 0;
                }
            }
#endif
        }
        protected override void OnDisable()
        {
            HotFixInit.Update -= Update;
            HotFixInit.FixedUpdate -= FixedUpdate;
            MonoNetMsgLooper.Instance.OnReVerify -= OnReVerify;
            battleStartNtf = null;
            postionAct = null;
            battleEndNtf = null;
            OnLobbyRoomLeaveNtf = null;
        }
        async void OnReVerify()
        {
            MonoNetMsgLooper.Instance.LobbyJoinRoomRsp = null;
            MonoNetMsgLooper.Instance.NetClients.TryRemove(NetServer.Battle, out _);
            battleClient.Close();
            await Addressables.LoadSceneAsync("Assets/Bundles/Scenes/LobbyScene.unity").Task;
        }
        protected override void OnDestory()
        {
        }
        private async void SendSelfPos()
        {
            selfRb.targetVec3 = selfRgb.position;
            if (Vector3.SqrMagnitude(selfRb.lastVec3 - selfRb.targetVec3) < 0.0025f)
            {
                return;
            }
            await battleClient.Notify(new PositionNtf()
            {
                PlayerOrderInRoom = selfRb.orderInRoom,
                X = selfRgb.position.x,
                Z = selfRgb.position.z
            });
            selfRb.lastVec3 = selfRgb.position;
        }
        const int R = 160;
        const int R2 = R * R;
        public void OnBeginDrag(PointerEventData eventData)
        {
            OnDrag(eventData);
        }
        public void OnDrag(PointerEventData eventData)
        {
            PointerEventData da = (PointerEventData)eventData;
            moveImg.position = da.position;
            double angle = Vector2.SignedAngle(Vector2.right, moveImg.anchoredPosition);
            if (angle > 45 && angle <= 135)
            {
                dir = 1;
            }
            else if (angle > -135 && angle <= -45)
            {
                dir = 2;
            }
            else if (angle > 135 || angle <= -135)
            {
                dir = 3;
            }
            else
            {
                dir = 4;
            }
            if (Vector2.SqrMagnitude(moveImg.anchoredPosition) > R2)
            {
                angle = Math.PI * angle / 180;
                moveImg.anchoredPosition = new Vector2(R * (float)Math.Cos(angle), R * (float)Math.Sin(angle));
            }
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            moveImg.anchoredPosition = Vector3.zero;
            dir = 0;
        }
    }
    class Robot
    {
        public bool isSelf;
        public int orderInRoom;
        public Transform playerTr;
        public TextMesh name;
        public Vector3 lastVec3;
        public Vector3 targetVec3;
    }
}