namespace RJCP.Diagnostics.Config.CrashReporter
{
    /// <summary>
    /// Watchdog specific configuration.
    /// </summary>
    public sealed class WatchdogConfig
    {
        internal WatchdogConfig()
        {
            Overrides = new WatchdogOverrides();
            Ping = new WatchdogPing();
        }

        internal WatchdogConfig(Watchdog config)
        {
            Overrides = new WatchdogOverrides(config.Overrides);
            Ping = new WatchdogPing(config.WatchdogPing);
        }

        /// <summary>
        /// Gets the watchdog overrides for watchdogs.
        /// </summary>
        /// <value>The watchdog overrides.</value>
        public WatchdogOverrides Overrides { get; private set; }

        /// <summary>
        /// Gets the watchdog ping configuration.
        /// </summary>
        /// <value>The watchdog ping.</value>
        public WatchdogPing Ping { get; private set; }
    }
}
