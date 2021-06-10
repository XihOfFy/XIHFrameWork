using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XIHBasic;
namespace XIHHotFix
{
    public class BattleSceneMgr : AbsComponent
    {
        protected BattleSceneMgr(MonoTouch dot) : base(dot) { }
        private GameObject playerPrefab;
        private Rigidbody selfRgb;
        private Transform followCam;
        private Vector3 deltaCam;
        private RectTransform moveImg;
        private Text cdTimeTx;
        private Button exitBtn;
        private int dir = 0;
        private Dictionary<int, Vector3> dirSpeed;
        protected async override void Awake()
        {
            MonoTouch dot = MonoDot as MonoTouch;
            dot.onBeginDrag = OnBeginDrag;
            dot.onDrag = OnDrag;
            dot.onEndDrag = OnEndDrag;
            playerPrefab = dot.GameObjsDic["Player"];
            followCam = dot.GameObjsDic["PlayerCamera"].transform;
            moveImg = dot.GameObjsDic["Point"].GetComponent<RectTransform>();
            cdTimeTx = dot.GameObjsDic["CD"].GetComponent<Text>();
            exitBtn = dot.GameObjsDic["Exit"].GetComponent<Button>();
            deltaCam = followCam.position;
            dirSpeed = new Dictionary<int, Vector3>();
            dirSpeed.Add(0, Vector3.zero);
            int speed = 8;
            dirSpeed.Add(1, Vector3.forward * Time.fixedDeltaTime * speed);
            dirSpeed.Add(2, Vector3.back * Time.fixedDeltaTime * speed);
            dirSpeed.Add(3, Vector3.left * Time.fixedDeltaTime * speed);
            dirSpeed.Add(4, Vector3.right * Time.fixedDeltaTime * speed);
            bool isBacking = false;
            exitBtn.onClick.AddListener(async () =>
            {
                if (isBacking) return;
                isBacking = true;
                await Addressables.LoadSceneAsync("Assets/Bundles/Scenes/LoginScene.unity").Task;
            });

            string playerName = "Player";
            var gb = GameObject.Instantiate(playerPrefab);
            gb.SetActive(true);
            gb.name = playerName;
            gb.GetComponentInChildren<TextMesh>().text = playerName;
            gb.GetComponent<Collider>().isTrigger = false;
            selfRgb = gb.AddComponent<Rigidbody>();
            selfRgb.constraints = RigidbodyConstraints.FreezePositionY;
            selfRgb.freezeRotation = true;
            selfRgb.useGravity = false;

            string sceneName = SceneManager.GetActiveScene().name;
            var desT = DateTime.Now.AddMilliseconds(30000);
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
            if (!isBacking && SceneManager.GetActiveScene().name == sceneName)
            {
                await Addressables.LoadSceneAsync("Assets/Bundles/Scenes/LoginScene.unity").Task;
            }
        }

        Vector3 refV3;
        protected override void OnEnable()
        {
            HotFixInit.Update += Update;
            HotFixInit.FixedUpdate += FixedUpdate;
        }
        private void FixedUpdate()
        {
            if (selfRgb == null) return;
            selfRgb.MovePosition(Vector3.SmoothDamp(selfRgb.position, selfRgb.position + dirSpeed[dir], ref refV3, 0.03125f));
            followCam.position = Vector3.Lerp(followCam.position, selfRgb.position + deltaCam, 0.25f);
        }

        private void Update()
        {
            /*不能使用宏定义，除非dll生成也分平台
            if (Application.platform==RuntimePlatform.WindowsEditor &&  Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    dir = 1;
                }
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    dir = 2;
                }
                else if (Input.GetKey(KeyCode.LeftArrow))
                {
                    dir = 3;
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    dir = 4;
                }
                else
                {
                    dir = 0;
                }
            }*/
        }
        protected override void OnDisable()
        {
            HotFixInit.Update -= Update;
            HotFixInit.FixedUpdate -= FixedUpdate;
        }
        protected override void OnDestory()
        {
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
}