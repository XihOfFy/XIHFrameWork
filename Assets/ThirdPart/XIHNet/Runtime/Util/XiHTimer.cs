using System;

namespace XiHNet
{
    public class XiHTimer : System.Timers.Timer
    {
        private Action _timeEvent;

        public void SetTimer(Action onTimedEvent, int interval)
        {
            interval = interval == 0 ? 1 : interval;

            this.Stop();
            this.Elapsed -= OnElapsedEvent;
            this._timeEvent = onTimedEvent;
            this.Interval = interval;
            this.Elapsed += OnElapsedEvent;
            this.AutoReset = true;
            this.Start();
        }

        public void SetTimeout(Action onTimedEvent, int interval)
        {
            interval = interval == 0 ? 1 : interval;

            this.Elapsed -= OnElapsedEvent;
            this._timeEvent = onTimedEvent;
            this.Interval = interval;
            this.Elapsed += OnElapsedEvent;
            this.AutoReset = false;
            this.Start();
        }

        private void OnElapsedEvent(object sender, EventArgs e)
        {
            this._timeEvent();
        }
    }
}
