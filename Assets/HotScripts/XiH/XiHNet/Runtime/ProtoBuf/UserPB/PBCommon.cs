using ProtoBuf;

namespace XiHNet {
    [ProtoContract]
    [MsgTypeCode(1000,false)]
    public sealed partial class Ping : IMessage
    {
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [ProtoContract]
    [MsgTypeCode(1001,true)]
    public sealed partial class Pong : IMessage
    {
        [ProtoMember(1)]
        public long ServerUtcTicks { get; set; }
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [ProtoContract]
    [MsgTypeCode(60000,false)]
    public sealed partial class KickOutNtf : IMessage
    {
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
}