namespace RJCP.Diagnostics.Watchdog.Timer
{
    using System;

    public class VirtualTimerSource : ITimerSource
    {
        private volatile int m_Time;

        public VirtualTimerSource(int time)
        {
            m_Time = time;
        }

        public int GetClock()
        {
            return m_Time;
        }

        public void UpdateClock(int time)
        {
            ThrowHelper.ThrowIfNegative(time);

            unchecked {
                m_Time += time;
            }
            OnClockUpdatedEvent(this, new EventArgs());
        }

        public event EventHandler ClockUpdatedEvent;

        protected virtual void OnClockUpdatedEvent(object sender, EventArgs args)
        {
            EventHandler handler = ClockUpdatedEvent;
            if (handler is not null) handler(sender, args);
        }
    }
}
