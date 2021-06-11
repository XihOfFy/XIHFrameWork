
using System.Collections.Generic;
using XiHNet;

namespace XIHServer
{
    public struct UserBean
    {
        public string Key => loginType == LoginType.LoginByGm ? account : uniqueid;
        public LoginType loginType;
        public string account;
        public string password;
        public string uniqueid;
        public string name;
    }
    public class MockDB : IDatabase
    {
        private readonly Dictionary<string, UserBean> userdb = new Dictionary<string, UserBean>();
        public UserBean? Login(LoginReq req)
        {
            switch ((LoginType)req.LoginType)
            {
                case LoginType.LoginByGm:
                    if (string.IsNullOrEmpty(req.Account) || string.IsNullOrEmpty(req.Password))
                    {
                        return null;
                    }
                    if (userdb.ContainsKey(req.Account) && userdb[req.Account].password.Equals(req.Password)) return userdb[req.Account];
                    break;
                case LoginType.LoginBySDK:
                    if (string.IsNullOrEmpty(req.UniqueId))
                    {
                        return null;
                    }
                    RegisterAccount((LoginType)req.LoginType, req.UniqueId);
                    return userdb[req.UniqueId];
            }
            return null;
        }
        private bool RegisterAccount(LoginType loginType, params string[] apu)
        {
            lock (userdb)
            {
                if (userdb.ContainsKey(apu[0])) return false;
                switch (loginType)
                {
                    case LoginType.LoginByGm:
                        if (apu[0].Replace(" ", "") == "" || apu[1].Replace(" ", "") == "")
                        {
                            return false;
                        }
                        userdb.Add(apu[0], new UserBean() { loginType = loginType, account = apu[0], password = apu[1], uniqueid = "", name = $"acc_{userdb.Count}" });
                        break;
                    case LoginType.LoginBySDK:
                        if (apu[0].Replace(" ", "") == "")
                        {
                            return false;
                        }
                        userdb.Add(apu[0], new UserBean() { loginType = loginType, account = "", password = "", uniqueid = apu[0], name = $"sdk_{userdb.Count}" });
                        break;
                }
            }
            return true;
        }

        public bool RegisterAccount(RegisterReq req)
        {
            if (string.IsNullOrEmpty(req.Account) || string.IsNullOrEmpty(req.Password))
            {
                return false;
            }
            return RegisterAccount(LoginType.LoginByGm, req.Account, req.Password);
        }

        public UserBean? QueryUser(string accOrUid)
        {
            if (!string.IsNullOrEmpty(accOrUid)&&userdb.ContainsKey(accOrUid)) return userdb[accOrUid];
            return null;
        }
    }
}