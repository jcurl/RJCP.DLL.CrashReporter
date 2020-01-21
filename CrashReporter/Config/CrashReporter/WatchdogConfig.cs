namespace RJCP.Diagnostics.Config.CrashReporter
{
    internal class WatchdogConfig
    {
        public WatchdogConfig()
        {
            Overrides = new WatchdogOverrides();
        }

        public WatchdogConfig(Watchdog config)
        {
            Overrides = new WatchdogOverrides(config.Overrides);
        }

        public WatchdogOverrides Overrides { get; private set; }
    }
}
