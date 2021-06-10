using ProtoBuf;
using System.Collections.Generic;

namespace XiHNet
{
    [MsgTypeCode(20000, false)]
    [ProtoContract]
    public sealed partial class LobbyVerifyReq : IMessage
    {
        [ProtoMember(1)]
        public ulong SessionKey { get; set; }
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [MsgTypeCode(20001, true)]
    [ProtoContract]
    public sealed partial class LobbyVerifyRsp : IMessage
    {
        [ProtoMember(1)]
        public bool Result { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [MsgTypeCode(20002, false)]
    [ProtoContract]
    public sealed partial class LobbyChatNtf : IMessage
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Info { get; set; }
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [MsgTypeCode(20003, false)]
    [ProtoContract]
    public sealed partial class LobbyCreateRoomReq : IMessage
    {
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [MsgTypeCode(20004, true)]
    [ProtoContract]
    public sealed partial class LobbyCreateRoomRsp : IMessage
    {
        [ProtoMember(1)]
        public bool Result { get; set; }
        [ProtoMember(2)]
        public ulong RoomId { get; set; }
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [MsgTypeCode(20005, false)]
    [ProtoContract]
    public sealed partial class LobbyJoinRoomReq : IMessage
    {
        [ProtoMember(1)]
        public ulong RoomId { get; set; }
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [MsgTypeCode(20006, true)]
    [ProtoContract]
    public sealed partial class LobbyJoinRoomRsp : IMessage
    {
        [ProtoMember(1)]
        public ulong RoomId { get; set; }
        [ProtoMember(2)]
        public int IdxInRoom { get; set; }
        [ProtoMember(3)]
        public int OwnerIdxInRoom { get; set; }
        [ProtoMember(4)]
        public string[] PlayersName { get; set; }
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [MsgTypeCode(20008, false)]
    [ProtoContract]
    public sealed partial class LobbyLeaveRoomReq : IMessage
    {
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [MsgTypeCode(20009, true)]
    [ProtoContract]
    public sealed partial class LobbyLeaveRoomRsp : IMessage
    {
        [ProtoMember(1)]
        public bool Result { get; set; }
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [MsgTypeCode(20007, false)]
    [ProtoContract]
    public sealed partial class LobbyChatRoomNtf : IMessage
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Info { get; set; }
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [ProtoContract]
    [MsgTypeCode(20010, false)]
    public sealed partial class LobbyRoomJoinNtf : IMessage
    {
        [ProtoMember(1)]
        public int OrderInRoom { get; set; }
        [ProtoMember(2)]
        public string PlayerName { get; set; }
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [ProtoContract]
    [MsgTypeCode(20011, false)]
    public sealed partial class LobbyRoomLeaveNtf : IMessage
    {
        [ProtoMember(1)]
        public int OrderInRoom { get; set; }
        [ProtoMember(2)]
        public int OwnerIdxInRoom { get; set; }

        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [ProtoContract]
    [MsgTypeCode(20012, false)]
    public sealed partial class LobbyGetRoomsReq : IMessage
    {
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [MsgTypeCode(20013, true)]
    [ProtoContract]
    public sealed partial class LobbyGetRoomsRsp : IMessage
    {
        [ProtoMember(1)]
        public RoomInfo[] Rooms { get; set; }
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [ProtoContract]
    public struct RoomInfo {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public ulong RoomId { get; set; }
    }
    [ProtoContract]
    [MsgTypeCode(20014, false)]
    public sealed partial class LobbyStartNtf : IMessage
    {
        [ProtoMember(1)]
        public string MapHost { get; set; }
        [ProtoMember(2)]
        public int MapPort { get; set; }
        [ProtoMember(3)]
        public int NetProtocol { get; set; }
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
}