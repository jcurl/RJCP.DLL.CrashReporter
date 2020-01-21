namespace RJCP.Diagnostics.Config.CrashReporter
{
    /// <summary>
    /// A collection of watchdog overrides.
    /// </summary>
    public class WatchdogOverrides
    {
        private Config.WatchdogOverrides m_Overrides;

        internal WatchdogOverrides() { }

        internal WatchdogOverrides(Config.WatchdogOverrides wdOverrides)
        {
            m_Overrides = wdOverrides;
        }

        /// <summary>
        /// Gets the <see cref="WatchdogOverride"/> with the specified task name.
        /// </summary>
        /// <param name="name">The task name.</param>
        /// <returns>Information about the watchdog override.</returns>
        public WatchdogOverride this[string name]
        {
            get
            {
                if (m_Overrides == null || name == null) return null;
                return new WatchdogOverride(m_Overrides[name]);
            }
        }

        /// <summary>
        /// Tries to get the override information for a watchdog task name.
        /// </summary>
        /// <param name="name">The task name.</param>
        /// <param name="wdOverride">The watchdog override information.</param>
        /// <returns>
        /// Returns <see langword="true"/> if there is an override for the watchdog task name in which case
        /// <paramref name="wdOverride"/> is set containing the information, <see langword="false"/> in which
        /// <paramref name="wdOverride"/> is set to <see langword="null"/>.
        /// </returns>
        public bool TryGetOverride(string name, out WatchdogOverride wdOverride) {
            if (m_Overrides == null) {
                wdOverride = null;
                return false;
            }
            wdOverride = this[name];
            return wdOverride != null;
        }
    }
}
