using Hot;
using ProtoBuf;
using System.Collections.Generic;

namespace XiHNet
{
    [ProtoContract]
    [MsgTypeCode(1000, false)]
    public sealed partial class Ping : IMessage
    {
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
        public MsgRoute Route { get; set; }
        public ulong TargetSession { get; set; }
    }
    [ProtoContract]
    [MsgTypeCode(1001, true)]
    public sealed partial class Pong : IMessage
    {
        [ProtoMember(1)]
        public long ServerUtcTicks { get; set; }
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
        public MsgRoute Route { get; set; }
        public ulong TargetSession { get; set; }
    }
    [ProtoContract]
    [MsgTypeCode(60000, false)]
    public sealed partial class KickOutNtf : IMessage
    {
        public ushort TaskId { get; set; }
        public MsgRoute Route { get; set; }
        public ulong TargetSession { get; set; }
    }

    /// <summary>
    /// 玩家离开通知
    /// </summary>
    [ProtoContract]
    [MsgTypeCode(10003, false)]
    public sealed partial class PlayerLeaveNtf : IMessage
    {
        [ProtoMember(1)] public ulong SessionKey { get; set; }
        [ProtoMember(32)] public ushort TaskId { get; set; }
        public MsgRoute Route { get; set; }
        public ulong TargetSession { get; set; }
    }
}
