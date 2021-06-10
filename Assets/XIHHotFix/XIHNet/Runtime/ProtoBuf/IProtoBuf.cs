using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XiHNet
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MsgTypeCodeAttribute : Attribute
    {
        public const ushort SKIP_PUTIN_QUE = 9999;
        public bool IsResponse { get; }
        public ushort MsgTypeCode { get; }
        /// <summary>
        /// 需保证每个<see cref="IMessage"/>的实现类的该属性<see cref="MsgTypeCode"/>值都不一致,可以按照服务器功能划分
        /// 协议类型 - 客户端针对小于 <see cref="SKIP_PUTIN_QUE"/> 的类型将直接处理，不放入队列(一般与UI或IO操作无关，纯内存数据操作的信号)
        /// 客户端针对大于 <see cref="SKIP_PUTIN_QUE"/> 的类型将放入队列不处理，需客户端自行轮询处理，可放在MonoBehaviour的Update方法中
        /// while (NetAdapter.ActQue.Count > 0)
        /// {
        ///     NetAdapter.ActQue.Dequeue().Invoke();
        /// }
        /// SomeReq: 客户端发送给服务端的请求
        /// SomeRsp: 服务端返回给客户端的响应
        /// SomeNtf: 通知信息
        /// </summary>
        public MsgTypeCodeAttribute(ushort code,bool isRsp) {
            MsgTypeCode = code;
            IsResponse = isRsp;
        }
    }
    public interface IMessage
    {
        /// <summary>
        /// 客户端完全不要管这个ID，将自增
        /// 服务器针对响应Response,将使用该ID并与请求Request一致，返回给客户端
        /// </summary>
        ushort TaskId { get; set; }
    }
    public sealed class NullMessage : IMessage
    {
        public static NullMessage IMessageNull { get; } = new NullMessage();
        public ushort TaskId { get => 0; set => _ = 0; }
        public string Msg { get; } = "Failed";
        public NullMessage() { }
        public NullMessage(string err) {
            Msg = err;
        }
    }
    public static class IMessageExt {
#if !XIHSERVER
        public static void Init() {
        }
#endif
        private static readonly Dictionary<ushort, Type> msg2Types = new Dictionary<ushort, Type>();//映射MsgType与Type
        private static readonly Dictionary<Type, ushort> type2Msgs = new Dictionary<Type, ushort>();//映射Type与MsgType
        private static readonly HashSet<ushort> rsps = new HashSet<ushort>();
        static IMessageExt() {
            Type mt = typeof(IMessage);
#if XIHSERVER
            var types = mt.Assembly.GetTypes();
#else
            var types = XIHBasic.HotFixBridge.HotfixTypes;
#endif
            foreach (Type type in types)
            {
                if (type.IsAbstract || type.IsInterface || type.IsEnum || !type.IsSealed)
                {
                    continue;
                }
                if (!mt.IsAssignableFrom(type)) {//接口
                    continue;
                }
                object[] objects = type.GetCustomAttributes(typeof(MsgTypeCodeAttribute), false);
                if (objects.Length == 0)
                {
                    continue;
                }
                if (objects[0] is MsgTypeCodeAttribute attr) {
                    ushort val = attr.MsgTypeCode;
                    if (msg2Types.ContainsKey(val))
                    {
                        Debugger.Log($"<color=red>{msg2Types[val].FullName}与{type.FullName}类型的值一致【{val}】，将跳过此设置</color>");
                        continue;
                    }
                    msg2Types.Add(val, type);
                    type2Msgs.Add(type, val);
                    //Debugger.Log($"{val}:{type}");
#if !XIHSERVER
                    ProtoBuf.PType.RegisterType(type.FullName,type);
#endif
                    if (attr.IsResponse) {
                        rsps.Add(val);
                    }
                }
            }
        }
        public static bool IsRespone(ushort type) => rsps.Contains(type);
        public static Type GetClassType(ushort msgType) {
            if (msg2Types.ContainsKey(msgType))
            {
                return msg2Types[msgType];
            }
            else {
                throw new TypeAccessException($"<color=red>{nameof(msg2Types)}不包含值为{msgType}所对应类型</color>");
            }
        }
        public static ushort GetMsgType<Rsp>() {
            Type type = typeof(Rsp);
            if (type2Msgs.ContainsKey(type)) {
                return type2Msgs[type];
            }
            else {
                throw new TypeAccessException($"<color=red>{nameof(type2Msgs)}不包含{type}类型</color>");
            }
        }
        public static ushort GetMsgType(this IMessage self) {
            Type type = self.GetType();
            if (type2Msgs.ContainsKey(type))
            {
                return type2Msgs[type];
            }
            else {
                throw new TypeAccessException($"<color=red>{nameof(type2Msgs)}枚举中不包含{type}类型</color>");
            }
        } 
    }
    
}