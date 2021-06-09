using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using XiHNet;

namespace XIHServer
{
    public static class BattleHandle
    {
        public static void BattleReady(AbsNetClient client, BattleReadyNtf ntf)
        {
            if (MockCache.Rooms.ContainsKey(ntf.RoomId)) {
                Room room = MockCache.Rooms[ntf.RoomId];
                if (ntf.PlayerOrderInRoom < room.players.Length) {
                    var player = room.players[ntf.PlayerOrderInRoom];
                    if (player != null && player.sessionKey == ntf.SessionKey) {
                        var map = room.map;
                        if (map != null) {
                            map.robots[ntf.PlayerOrderInRoom].btCli = client;
                            client.Map = map;
                            map.StartBattle();
                            client.AuthStatus = ClientAuth.Authed;
                        }
                    }
                }
            }
        }
        public static void PositionNtf(AbsNetClient client, PositionNtf ntf)
        {
            if (client.Map == null) return;

            foreach (var rb in client.Map.robots.Values) {
                if (rb.orderInRoom == ntf.PlayerOrderInRoom) continue;
                rb.btCli.Send(ntf);
            }
        }
        public static void BindHandle(Dictionary<ushort, Action<AbsNetClient, byte[]>> handles)
        {
            CommonHandle.IgnoreAuth.Add(IMessageExt.GetMsgType<BattleReadyNtf>());

            CommonHandle.Handle<BattleReadyNtf>(handles, BattleReady);
            CommonHandle.Handle<PositionNtf>(handles, PositionNtf);
        }
    }
}