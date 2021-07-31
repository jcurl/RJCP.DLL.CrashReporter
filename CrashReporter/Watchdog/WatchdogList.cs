namespace RJCP.Diagnostics.Watchdog
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    internal class WatchdogList
    {
        private readonly Timer.TimerList m_WarningTimer;
        private readonly Timer.TimerList m_CriticalTimer;
        private readonly Dictionary<string, WatchdogData> m_Timers = new Dictionary<string, WatchdogData>();

        public WatchdogList()
        {
            m_WarningTimer = new Timer.TimerList();
            m_CriticalTimer = new Timer.TimerList();
        }

        public WatchdogList(Timer.ITimerSource timerSource)
        {
            m_WarningTimer = new Timer.TimerList(timerSource);
            m_CriticalTimer = new Timer.TimerList(timerSource);
        }

        public bool Add(string name, int warningTimeout, int criticalTimeout)
        {
            if (m_Timers.ContainsKey(name)) return false;

            WatchdogData data = new WatchdogData() {
                WarningTimeout = warningTimeout,
                CriticalTimeout = criticalTimeout,
                RegisterTime = DateTime.Now,
                RegisterThread = Thread.CurrentThread,
                RegisterStack = new StackTrace(1, true),
            };
            m_WarningTimer.Add(name, warningTimeout);
            m_CriticalTimer.Add(name, criticalTimeout);
            m_Timers.Add(name, data);
            return true;
        }

        public bool Remove(string name)
        {
            if (!m_Timers.ContainsKey(name)) return false;
            m_WarningTimer.Remove(name);
            m_CriticalTimer.Remove(name);
            m_Timers.Remove(name);
            return true;
        }

        public bool Reset(string name, bool captureStack)
        {
            if (!m_Timers.TryGetValue(name, out WatchdogData data)) return false;
            m_WarningTimer.Change(name, data.WarningTimeout);
            m_CriticalTimer.Change(name, data.CriticalTimeout);
            data.LastPingTime = DateTime.Now;
            data.LastPingThread = Thread.CurrentThread;
            data.LastPingStack = captureStack ? new StackTrace(1, true) : null;
            return true;
        }

        public int GetNextExpiry()
        {
            int warningExpiry = m_WarningTimer.NextExpiryOffset();
            int criticalExpiry = m_CriticalTimer.NextExpiryOffset();

            if (warningExpiry == Timeout.Infinite) return criticalExpiry;
            if (criticalExpiry == Timeout.Infinite) return warningExpiry;
            return Math.Min(warningExpiry, criticalExpiry);
        }

        public IEnumerable<string> GetWarnings()
        {
            return m_WarningTimer.ExpungeExpired();
        }

        public IEnumerable<string> GetCriticalTimeouts()
        {
            return m_CriticalTimer.ExpungeExpired();
        }

        public WatchdogData this[string name]
        {
            get
            {
                if (!m_Timers.TryGetValue(name, out WatchdogData data)) return null;
                return data;
            }
        }
    }
}
