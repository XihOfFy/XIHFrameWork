using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using XIHBasic;
using XiHNet;

namespace XIHHotFix
{
    public class MonoNetMsgLooper : AbsComponent
    {
        private MonoNetMsgLooper(MonoDotBase dot) : base(dot) { }
        public bool IsFoucus { get; private set; }
        public LoginRsp LoginRsp { get; set; }
        public LobbyJoinRoomRsp LobbyJoinRoomRsp { get; set; }
        public LobbyVerifyRsp VerifyRsp { get; set; }
        public event Action OnReVerify;
        public void NotifyOnReVerify()
        {
            OnReVerify?.Invoke();
        }
        private static MonoNetMsgLooper instance;
        public static MonoNetMsgLooper Instance => instance;
        public ConcurrentDictionary<NetServer, NetAdapter> NetClients { get; private set; }
        void Update()
        {
            var ls = NetClients.Values;
            foreach (var client in ls)
            {
                while (client.ActQue.Count > 0)
                {
                    client.ActQue.Dequeue().Invoke();
                }
            }
        }
        public void Clear()
        {
            var clients = Instance.NetClients.Values;
            Instance.NetClients.Clear();
            foreach (var client in clients)
            {
                client.Close();
            }
            LoginRsp = null;
            LobbyJoinRoomRsp = null;
            VerifyRsp = null;
            OnReVerify = null;
        }

        protected override void Awake()
        {
            if (instance != null)
            {
                GameObject.Destroy(MonoDot.gameObject);
                return;
            }
            instance = this;
            GameObject.DontDestroyOnLoad(MonoDot.gameObject);
            NetClients = new ConcurrentDictionary<NetServer, NetAdapter>();
            HotFixInit.Update += Update;
        }

        protected override void OnEnable()
        {
        }

        protected override void OnDisable()
        {
        }

        protected override void OnDestory()
        {
            if (instance == this) HotFixInit.Update -= Update;
        }
    }
}
