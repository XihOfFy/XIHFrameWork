using XiHNet;

namespace XIHServer
{
    public interface IDatabase
    {
        public static IDatabase CurDBHelper = new MockDB();
        UserBean? Login(LoginReq req);
        UserBean? QueryUser(string accOrUid);
        bool RegisterAccount(RegisterReq apu);
    }
}
