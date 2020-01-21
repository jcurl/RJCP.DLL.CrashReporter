namespace RJCP.Diagnostics.Config.CrashReporter
{
    internal class WatchdogOverride
    {
        private Config.WatchdogOverride m_Override;

        public WatchdogOverride(Config.WatchdogOverride wdOverride)
        {
            m_Override = wdOverride;
        }

        public string Task
        {
            get { return m_Override.Task; }
        }

        public int WarningTimeout
        {
            get { return m_Override.WarningTimeout; }
        }

        public int CriticalTimeout
        {
            get { return m_Override.CriticalTimeout; }
        }
    }
}
