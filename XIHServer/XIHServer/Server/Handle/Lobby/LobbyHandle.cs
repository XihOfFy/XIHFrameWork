using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XiHNet;

namespace XIHServer
{
    public static class LobbyHandle
    {
        //正式环境断线重连应该使用新的协议，不能共用！！！最好保持最新SessionKey，避免剔除玩家正处于断线重连状态将新玩家踢出
        public async static void LobbyVerify(AbsNetClient client, LobbyVerifyReq req)
        {
            OnLineLobbyPlayer player = null;
            AbsNetClient other = null;
            if (MockCache.WaitVerify.ContainsKey(req.SessionKey))
            {
                var key = MockCache.WaitVerify[req.SessionKey];
                if (MockCache.LobbyOnLines.ContainsKey(key))
                {
                    player = MockCache.LobbyOnLines[key];
                    other = player.Client;
                    if (player.Room != null)
                    {
                        player.Room.Leave(player);
                    }
                }
                else
                {
                    var user = IDatabase.CurDBHelper.QueryUser(key);
                    player = new OnLineLobbyPlayer() { key = key, name = user.Value.name };
                    MockCache.LobbyOnLines.Add(key, player);
                }
                player.sessionKey = req.SessionKey;
                player.Client = client;
                client.SessionKey = req.SessionKey;
                client.Player = player;
                client.AuthStatus = ClientAuth.Authed;
                player.IsOnLine = true;
            }
            Debugger.Log($"WaitVerify:{string.Join(",", MockCache.WaitVerify.Select(s => s.Key))}");
            Debugger.Log($"LobbyOnLines:{string.Join(",", MockCache.LobbyOnLines.Select(s => s.Value.sessionKey))}  other.SessionKey:{other?.SessionKey}");
            client.Send(new LobbyVerifyRsp()
            {
                TaskId = req.TaskId,
                Name = player == null ? "" : player.name,
                Result = player != null
            });
            if (other != null)
            {
                if (other.SessionKey == req.SessionKey)
                {
                    other.AuthStatus = ClientAuth.Replaced;
                }
                else {
                    other.AuthStatus = ClientAuth.Outdated;
                }
                other.Send(new KickOutNtf());
                await Task.Delay(NetConfig.RecTimeOut);
                other.Close();
            }
        }
        public static void LobbyChat(AbsNetClient client, LobbyChatNtf ntf)
        {
            if (client.Player == null) return;
            foreach (var pl in MockCache.LobbyOnLines)
            {
                pl.Value.Client?.Send(ntf);
            }
        }
        public static void LobbyChatRoom(AbsNetClient client, LobbyChatRoomNtf ntf)
        {
            if (client.Player == null || client.Player.Room == null) return;
            foreach (var pl in client.Player.Room.players)
            {
                if (pl == null) continue;
                pl.Client?.Send(ntf);
            }
        }
        public static void LobbyCreateRoom(AbsNetClient client, LobbyCreateRoomReq req)
        {
            var player = client.Player;
            if (player == null) return;
            if (player.Room != null) player.Room.Leave(player);
            ulong key;
            if (MockCache.Rooms.ContainsKey(player.sessionKey))
            {
                key = IdGenerater.CreateUniqueId();
                MockCache.Rooms.Add(key, new Room(key, player));
            }
            else
            {
                key = player.sessionKey;
                MockCache.Rooms.Add(player.sessionKey, new Room(player.sessionKey, player));
            }
            client.Send(new LobbyCreateRoomRsp() { TaskId = req.TaskId, Result = true, RoomId = key });
        }
        public static void LobbyJoinRoom(AbsNetClient client, LobbyJoinRoomReq req)
        {
            var palyer = client.Player;
            if (palyer == null) return;
            if (palyer.Room != null) palyer.Room.Leave(palyer);
            int idx = -1;
            if (MockCache.Rooms.ContainsKey(req.RoomId))
            {
                idx = MockCache.Rooms[req.RoomId].Join(palyer);
            }
            string[] names = null;
            int ownerIdxInRoom = -1;
            if (idx != -1)
            {
                var ps = MockCache.Rooms[req.RoomId].players;
                names = new string[ps.Length];
                for (int i = 0; i < ps.Length; ++i)
                {
                    names[i] = ps[i] == null ? "" : ps[i].name;
                }
                ownerIdxInRoom = MockCache.Rooms[req.RoomId].OwnerId;
            }
            client.Send(new LobbyJoinRoomRsp() { TaskId = req.TaskId,RoomId=req.RoomId, IdxInRoom = idx, OwnerIdxInRoom = ownerIdxInRoom, PlayersName = names });
        }
        public static void LobbyLeaveRoom(AbsNetClient client, LobbyLeaveRoomReq req)
        {
            var palyer = client.Player;
            if (palyer == null) return;
            bool res = palyer.Room != null;
            if (res) palyer.Room.Leave(palyer);
            client.Send(new LobbyLeaveRoomRsp() { TaskId = req.TaskId, Result = res });
        }
        public static void LobbyGetRooms(AbsNetClient client, LobbyGetRoomsReq req)
        {
            var palyer = client.Player;
            if (palyer == null) return;
            List<RoomInfo> rooms = new List<RoomInfo>();
            foreach (var r in MockCache.Rooms.Values)
            {
                rooms.Add(new RoomInfo() { Name = r.players[r.OwnerId].name, RoomId = r.id });
            }
            client.Send(new LobbyGetRoomsRsp() { TaskId = req.TaskId, Rooms = rooms });
        }
        public static void LobbyStartNtf(AbsNetClient client, LobbyStartNtf ntf)
        {
            var palyer = client.Player;
            if (palyer == null || palyer.Room == null) return;
            ntf.MapHost = Program.SvrCfg[NetServer.Battle].IPEndPoint.Address.ToString();
            ntf.MapPort = Program.SvrCfg[NetServer.Battle].IPEndPoint.Port;
            ntf.NetProtocol = Program.SvrCfg[NetServer.Battle].NetProtocol;
            palyer.Room.StartBattle(ntf);
        }
        public static void BindHandle(Dictionary<ushort, Action<AbsNetClient, byte[]>> handles)
        {
            CommonHandle.IgnoreAuth.Add(IMessageExt.GetMsgType<LobbyVerifyReq>());

            CommonHandle.Handle<LobbyVerifyReq>(handles, LobbyVerify);
            CommonHandle.Handle<LobbyChatNtf>(handles, LobbyChat);
            CommonHandle.Handle<LobbyChatRoomNtf>(handles, LobbyChatRoom);
            CommonHandle.Handle<LobbyCreateRoomReq>(handles, LobbyCreateRoom);
            CommonHandle.Handle<LobbyJoinRoomReq>(handles, LobbyJoinRoom);
            CommonHandle.Handle<LobbyLeaveRoomReq>(handles, LobbyLeaveRoom);
            CommonHandle.Handle<LobbyGetRoomsReq>(handles, LobbyGetRooms);
            CommonHandle.Handle<LobbyStartNtf>(handles, LobbyStartNtf);
        }
    }
}