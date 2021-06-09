
using System;
using System.Collections.Generic;
using System.IO;
using XiHNet;

namespace XIHServer
{
    public static class CommonHandle
    {
        public static HashSet<ushort> IgnoreAuth { get; } = new HashSet<ushort>();
        public static void Ping(AbsNetClient client, Ping req)
        {
            //Debugger.Log($"服务器响应Ping<<:【{req}】");
            client.Send(new Pong()
            {
                TaskId = req.TaskId,
                ServerUtcTicks = DateTimeOffset.UtcNow.Ticks
            });
        }
        public static void BindHandle(Dictionary<ushort, Action<AbsNetClient, byte[]>> handles)
        {
            CommonHandle.IgnoreAuth.Add(IMessageExt.GetMsgType<Ping>());
            Handle<Ping>(handles, Ping);
        }
        public static void Handle<T>(Dictionary<ushort, Action<AbsNetClient, byte[]>> handles, Action<AbsNetClient, T> act) where T : IMessage
        {
            handles[IMessageExt.GetMsgType<T>()] = (client, data) =>
            {
                using MemoryStream stream = new MemoryStream(data);
                act(client, ProtoBuf.Serializer.Deserialize<T>(stream));
            };
        }
    }
}
