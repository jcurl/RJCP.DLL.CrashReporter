namespace RJCP.Diagnostics.Config.CrashReporter
{
    /// <summary>
    /// Watchdog Override information.
    /// </summary>
    public class WatchdogOverride
    {
        private Config.WatchdogOverride m_Override;

        internal WatchdogOverride(Config.WatchdogOverride wdOverride)
        {
            m_Override = wdOverride;
        }

        /// <summary>
        /// Gets the name of the watchdog task that is being overridden.
        /// </summary>
        /// <value>The name of the watchdog task that is being overridden.</value>
        public string Task
        {
            get { return m_Override.Task; }
        }

        /// <summary>
        /// Gets the watchdog warning timeout.
        /// </summary>
        /// <value>The watchdog warning timeout in milliseconds (-1 is no timeout).</value>
        public int WarningTimeout
        {
            get { return m_Override.WarningTimeout; }
        }

        /// <summary>
        /// Gets the watchdog critical timeout.
        /// </summary>
        /// <value>The watchdog critical timeout in milliseconds (-1 is no timeout).</value>
        public int CriticalTimeout
        {
            get { return m_Override.CriticalTimeout; }
        }
    }
}
