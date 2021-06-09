using ProtoBuf;

namespace XiHNet
{
    [ProtoContract]
    public enum LoginType
    {
        LoginByGm,
        LoginBySDK
    }
    [ProtoContract]
    public enum LoginResultType
    {
        LoginResultSuccess,
        LoginResultError
    }
    [ProtoContract]
    [MsgTypeCode(10000, false)]
    public sealed partial class LoginReq : IMessage
    {
        [ProtoMember(1)]
        public string Account { get; set; }
        [ProtoMember(2)]
        public string Password { get; set; }
        [ProtoMember(3)]
        public LoginType LoginType { get; set; }
        [ProtoMember(4)]
        public string UniqueId { get; set; }
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [MsgTypeCode(10001, true)]
    [ProtoContract]
    public sealed partial class LoginRsp : IMessage
    {
        [ProtoMember(1)]
        public LoginResultType Result { get; set; }
        [ProtoMember(2)]
        public ulong SessionKey { get; set; }
        [ProtoMember(3)]
        public string GateHost { get; set; }
        [ProtoMember(4)]
        public int GatePort { get; set; }
        [ProtoMember(5)]
        public NetworkProtocol NetProtocol { get; set; }
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [ProtoContract]
    [MsgTypeCode(10002, false)]
    public sealed partial class RegisterReq : IMessage
    {
        [ProtoMember(1)]
        public string Account { get; set; }
        [ProtoMember(2)]
        public string Password { get; set; }
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
    [MsgTypeCode(10003, true)]
    [ProtoContract]
    public sealed partial class RegisterRsp : IMessage
    {

        [ProtoMember(1)]
        public bool Result { get; set; }
        [ProtoMember(32)]
        public ushort TaskId { get; set; }
    }
}
