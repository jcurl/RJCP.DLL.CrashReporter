namespace RJCP.Diagnostics.Config.CrashReporter
{
    /// <summary>
    /// Watchdog Ping Configuration Information.
    /// </summary>
    public class WatchdogPing
    {
        private WatchdogPingElement m_WatchdogPing;

        internal WatchdogPing() { }

        internal WatchdogPing(WatchdogPingElement wdPing)
        {
            m_WatchdogPing = wdPing;
        }

        private bool m_DisableStack;

        /// <summary>
        /// Gets or sets a value indicating whether a watchdog ping should capture stack information.
        /// </summary>
        /// <value>
        /// Returns <see langword="true"/> if stack information is automatically captured, <see langword="false"/>
        /// otherwise. Setting this value to <see langword="false"/> will override the application configuration and
        /// disable the stack capture, setting to <see langword="true"/> will return the value as provided by the
        /// application configuration.
        /// </value>
        public bool StackCapture
        {
            get
            {
                if (m_DisableStack) return false;

                // Default behavior is to capture the stack (i.e. return true if null).
                return m_WatchdogPing == null ||
                    m_WatchdogPing.StackCapture;
            }
            set
            {
                m_DisableStack = !value;
            }
        }
    }
}
