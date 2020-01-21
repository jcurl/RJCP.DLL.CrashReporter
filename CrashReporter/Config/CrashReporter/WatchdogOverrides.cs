namespace RJCP.Diagnostics.Config.CrashReporter
{
    internal class WatchdogOverrides
    {
        private Config.WatchdogOverrides m_Overrides;

        public WatchdogOverrides() { }

        public WatchdogOverrides(Config.WatchdogOverrides wdOverrides)
        {
            m_Overrides = wdOverrides;
        }

        public WatchdogOverride this[string name]
        {
            get
            {
                if (m_Overrides == null || name == null) return null;
                return new WatchdogOverride(m_Overrides[name]);
            }
        }

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
