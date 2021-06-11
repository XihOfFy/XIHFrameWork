using ProtoBuf;
using System.Collections.Generic;

namespace XiHNet
{
    [ProtoContract]
    [MsgTypeCode(30001, false)]
    public sealed partial class BattleReadyNtf : IMessage 
    {
        [ProtoMember(1)]
        public int PlayerOrderInRoom { get; set; }
        [ProtoMember(2)]
        public ulong RoomId { get; set; }
        [ProtoMember(3)]
        public ulong SessionKey { get; set; }
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [ProtoContract]
    [MsgTypeCode(30002, false)]
    public sealed partial class BattleStartNtf : IMessage 
    {
        [ProtoMember(1)]
        public PBRobot[] Robots { get; set; }
        [ProtoMember(2)]
        public int CDTime { get; set; }
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [ProtoContract]
    public sealed partial class PBRobot
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public int OrderInRoom { get; set; }
    }
    [ProtoContract]
    [MsgTypeCode(30003, false)]
    public sealed partial class BattleEndNtf : IMessage 
    {
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [ProtoContract]
    [MsgTypeCode(30000, false)]
    public sealed partial class PositionNtf : IMessage
    {
        [ProtoMember(1)]
        public int PlayerOrderInRoom { get; set; }
        [ProtoMember(2)]
        public float X { get; set; }
        [ProtoMember(3)]
        public float Z { get; set; }
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
}