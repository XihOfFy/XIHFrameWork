using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XiHNet;

namespace XIHServer
{
    public static class LoginHandle
    {
        public static void Register(AbsNetClient client, RegisterReq req)
        {
            bool res = IDatabase.CurDBHelper.RegisterAccount(req);
            client.Send(new RegisterRsp()
            {
                TaskId = req.TaskId,
                Result = res
            });
        }
        public static void Login(AbsNetClient client, LoginReq req)
        {
            UserBean? res = IDatabase.CurDBHelper.Login(req);
            if (res == null)
            {
                client.Send(new LoginRsp()
                {
                    TaskId = req.TaskId,
                    Result = LoginResultType.LoginResultError
                });
            }
            else
            {
                ulong sk = IdGenerater.CreateUniqueId();
                client.Send(new LoginRsp()
                {
                    TaskId = req.TaskId,
                    GateHost = Program.SvrCfg[NetServer.Lobby].IPEndPoint.Address.ToString(),
                    GatePort = Program.SvrCfg[NetServer.Lobby].IPEndPoint.Port,
                    NetProtocol = Program.SvrCfg[NetServer.Lobby].NetProtocol,
                    Result = LoginResultType.LoginResultSuccess,
                    SessionKey = sk
                });
                MockCache.WaitVerify.Add(sk, res.Value.Key);
                MockCache.RemoveSessionkey(res.Value.Key, sk);
            }
        }
        public static void BindHandle(Dictionary<ushort, Action<AbsNetClient, byte[]>> handles)
        {
            CommonHandle.IgnoreAuth.Add(IMessageExt.GetMsgType<RegisterReq>());
            CommonHandle.IgnoreAuth.Add(IMessageExt.GetMsgType<LoginReq>());

            CommonHandle.Handle<RegisterReq>(handles, Register);
            CommonHandle.Handle<LoginReq>(handles, Login);
        }
    }
}
