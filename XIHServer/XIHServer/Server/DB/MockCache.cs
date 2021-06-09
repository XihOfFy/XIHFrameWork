using System.Collections.Generic;
using System.Threading.Tasks;
using XiHNet;

namespace XIHServer
{
    public static class MockCache
    {
        public static Dictionary<ulong, string> WaitVerify { get; } = new Dictionary<ulong, string>();//sessionkey,key
        public static Dictionary<string, OnLineLobbyPlayer> LobbyOnLines { get; } = new Dictionary<string, OnLineLobbyPlayer>();
        public static Dictionary<ulong, Room> Rooms { get; } = new Dictionary<ulong, Room>();
        public static void RemovePlayer(AbsNetClient netClient,OnLineLobbyPlayer player , ulong sessionKey) {
            if (player == null) return;
            if (player.sessionKey == sessionKey)
            {
                if (netClient == player.Client)
                {//可能已经重连了，就不许删除,目前状态可能是 ClientAuth.Replaced/ClientAuth.Authed
                    player.IsOnLine = false;//可以通知，好友列表
                    LobbyOnLines.Remove(player.key);//可能等待重连
                    if (player.Room != null) player.Room.Leave(player);
                    RemoveSessionkey(player.key, sessionKey);
                }
            }
            else {
                WaitVerify.Remove(sessionKey);//被踢的玩家，直接移除
            }

        }
        public async static void RemoveSessionkey(string key, ulong sessionKey) {
            await Task.Delay(NetConfig.RecTimeOut * 10);
            if (MockCache.LobbyOnLines.ContainsKey(key))
            {
                var player = MockCache.LobbyOnLines[key];
                if (player.sessionKey != sessionKey) MockCache.WaitVerify.Remove(sessionKey);
            }
            else
            {
                MockCache.WaitVerify.Remove(sessionKey);
            }
        }
    }
    public class OnLineLobbyPlayer {
        public string key;
        public ulong sessionKey;
        public string name;
        public AbsNetClient Client { get; set; }
        public Room Room { get; set; }
        public bool IsOnLine { get; set; }
    }
    public class Room {
        const int MAX = 8;
        public ulong id;
        public OnLineLobbyPlayer[] players;
        public BattleMap map;
        readonly RoomOwnerList roomOwnerList;
        public int OwnerId => roomOwnerList.Owner.idx;
        public int Count => roomOwnerList.Count;
        public Room(ulong id ,OnLineLobbyPlayer owner) {
            this.id = id;
            players = new OnLineLobbyPlayer[MAX];
            players[0] = owner;
            owner.Room = this;
            roomOwnerList = new Room.RoomOwnerList(0);
        }
        public int Join(OnLineLobbyPlayer player) {
            if (map != null) return -1;//已是战斗中
            int idx = -1;
            if (Count == MAX|| Count == 0) return idx;
            for (int i = 0; i < MAX; ++i) {
                if (players[i] == null) {
                    foreach (var ps in players)
                    {
                        if (ps == null) continue;
                        ps.Client?.Send(new LobbyRoomJoinNtf() { OrderInRoom = i, PlayerName = player.name });
                    }
                    players[i] = player;
                    player.Room = this;
                    roomOwnerList.Add(i);
                    return i;
                }
            }
            return idx;
        }
        public int Leave(OnLineLobbyPlayer player) {
            int idx = -1;
            for (int i = 0; i < MAX; ++i)
            {
                if (players[i] != null && players[i].key==player.key)
                {
                    players[i] = null;
                    player.Room = null;
                    roomOwnerList.Remove(i);
                    if (map != null) map.Leave(i);
                    if (Count == 0)
                    {
                        RemoveRoom();
                    }
                    else {
                        foreach (var ps in players) {
                            if (ps == null) continue;
                            ps.Client?.Send(new LobbyRoomLeaveNtf() {OrderInRoom=i,OwnerIdxInRoom=OwnerId});
                        }
                    }
                    return i;
                }
            }
            return idx;
        }
        private void RemoveRoom() {
            MockCache.Rooms.Remove(id);
        }
        public void StartBattle(LobbyStartNtf ntf) {
            map = new BattleMap(this);
            for (int i = 0; i < MAX; ++i)
            {
                if (players[i] != null)
                {
                    map.robots.Add(i,new BattleMap.Robot() { name=players[i].name, orderInRoom = i});
                    players[i].Client?.Send(ntf);
                }
            }
        }
        public void EndBattle() {
            map = null;
        }
        internal class RoomOwnerList {
            public int Count { get; private set; }
            public RoomOwner Owner { get; private set; }
            public RoomOwnerList(int idx) {
                Owner = new RoomOwner() { idx=idx,pre=null,next=null };
                Count = 1;
            }
            public void Add(int idx) {
                RoomOwner root = Owner;
                while (root.next != null) {
                    root = root.next;
                }
                root.next = new RoomOwner() { idx=idx,pre=root,next=null};
                ++Count;
            }
            public void Remove(int idx)
            {
                RoomOwner root = Owner;
                while (root.idx != idx) {
                    root = root.next;
                }
                if (root.pre == null)
                {
                    Owner = root.next;
                    if (Owner != null)
                    {
                        Owner.pre = null;
                    }
                }
                else {
                    root.pre.next = root.next;
                    if (root.next != null) {
                        root.next.pre = root.pre;
                    }
                }
                --Count;
            }
        }
        internal class RoomOwner {
            public int idx;
            public RoomOwner pre;
            public RoomOwner next;
        }
    }
    public class BattleMap {
        //应该具备更多的功能，例如定时轮询。管理各个角色和客户端状态
        public Room room;
        public Dictionary<int, Robot> robots;
        private bool isStarted;
        public BattleMap(Room room) {
            this.room = room;
            robots = new Dictionary<int, Robot>();
            isStarted = false;
        }
        async void EndBattle(int cdTime) {
            await Task.Delay(cdTime);
            foreach (var rb in robots.Values)
            {
                rb.btCli?.Send(new BattleEndNtf());
				rb.btCli.Map = null;
            }
            robots.Clear();
            room.EndBattle();
            room = null;
            isStarted = false;
        }
        public void StartBattle() {
            if (isStarted) return;
            isStarted = true;
            foreach (var r in robots.Values) {
                isStarted &= (r.btCli != null);
            }
            if (isStarted) {
                List<PBRobot> rbs = new List<PBRobot>();
                foreach (var r in robots.Values)
                {
                    rbs.Add(new PBRobot() { Name=r.name,OrderInRoom=r.orderInRoom});
                }
                int cdTime = 30000;
                foreach (var r in robots.Values)
                {
                    r.btCli.Send(new BattleStartNtf() { Robots= rbs, CDTime= cdTime });
                }
                EndBattle(cdTime);
            }
        }
        public void Leave(int idx) {
            robots.Remove(idx);
            if(robots.Count>0) StartBattle();
        }
        public class Robot {
            public string name;
            public int orderInRoom;
            public AbsNetClient btCli;
        }
    }
}
