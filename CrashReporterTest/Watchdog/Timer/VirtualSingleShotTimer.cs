namespace RJCP.Diagnostics.Watchdog.Timer
{
    using System;
    using System.Threading;

    public sealed class VirtualSingleShotTimer : ISingleShotTimer
    {
        private VirtualTimerSource m_TimerSource;

        public VirtualSingleShotTimer(VirtualTimerSource timerSource)
        {
            if (timerSource == null) throw new ArgumentNullException(nameof(timerSource));
            m_TimerSource = timerSource;
            m_TimerSource.ClockUpdatedEvent += ClockUpdatedEvent;
        }

        private bool m_AlarmSet;
        private int m_Alarm;

        public void SetDelay(int timeout)
        {
            if (timeout < 0 && timeout != Timeout.Infinite)
                throw new ArgumentException("VirtualSingleShotTimer SetDelay with invalid timeout");

            if (timeout == Timeout.Infinite) {
                m_AlarmSet = false;
                return;
            }

            unchecked {
                m_Alarm = m_TimerSource.GetClock() + timeout;
            }
            m_AlarmSet = true;
        }

        public event EventHandler AlarmEvent;

        private void OnAlarmEvent(object sender, EventArgs args)
        {
            EventHandler handler = AlarmEvent;
            if (handler != null) AlarmEvent(sender, args);
        }

        private void ClockUpdatedEvent(object sender, EventArgs e)
        {
            if (!m_AlarmSet) return;

            unchecked {
                if ((m_Alarm - m_TimerSource.GetClock()) <= 0) {
                    m_AlarmSet = false;
                    OnAlarmEvent(this, new EventArgs());
                }
            }
        }
    }
}
