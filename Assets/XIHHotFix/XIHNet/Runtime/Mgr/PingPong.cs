using System;

namespace XiHNet
{
    public class PingPong
    {
        private readonly XiHTimer timer;
        private readonly long timeOutTime;
        private readonly Action timeOutAct;
        private readonly NetAdapter adapter;
        private long lastRecLocalUtcTicks;//上次收到响应时的本地时间   Ticks 1(毫微秒)=1豪秒*10000   一个Tick是100纳秒（1万Tick等于1毫秒）
        private long svrRspUtcTicks;//上传收到响应的服务器时间 UtcTicks
        public long ServerTimeUtcTicks => svrRspUtcTicks + (DateTimeOffset.UtcNow.Ticks - lastRecLocalUtcTicks);
        public PingPong(NetAdapter adapter, Action timeOutAct)
        {
            this.adapter = adapter;
            timer = new XiHTimer();
            timeOutTime = NetConfig.RecTimeOut * 10000;
            this.timeOutAct = timeOutAct;
        }
        public void Start()
        {
            timer.Stop();
            lastRecLocalUtcTicks = DateTimeOffset.UtcNow.Ticks;
            svrRspUtcTicks = lastRecLocalUtcTicks;
            timer.SetTimer(DoCheck, NetConfig.RecTimeOut >> 1);
        }
        public void Stop()
        {
            timer.Stop();
        }
        private async void DoCheck()
        {
            if (IsKeepConnect() && await adapter.Request(new Ping()) is Pong rsp)
            {
                lastRecLocalUtcTicks = DateTimeOffset.UtcNow.Ticks;
                svrRspUtcTicks = rsp.ServerUtcTicks;
            }
        }
        private bool IsKeepConnect()
        {
            //Debugger.Log($"current：{current}，lastRecTime：{lastRecTime}，ServerTime: {ServerTime}");
            if (DateTimeOffset.UtcNow.Ticks - lastRecLocalUtcTicks > timeOutTime)
            {
                Stop();
                adapter.ActQue.Enqueue(()=> { timeOutAct?.Invoke(); });
                return false;
            }
            return true;
        }
    }
}
