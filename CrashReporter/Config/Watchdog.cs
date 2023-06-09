namespace RJCP.Diagnostics.Config
{
    using System.Configuration;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "There isn't a strict relationship")]
    internal sealed class Watchdog : ConfigurationSection
    {
        [ConfigurationProperty("Overrides", IsRequired = false, IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(WatchdogOverrides))]
        public WatchdogOverrides Overrides
        {
            get { return (WatchdogOverrides)this["Overrides"]; }
        }

        [ConfigurationProperty("Ping", IsRequired = false)]
        public WatchdogPingElement WatchdogPing
        {
            get { return (WatchdogPingElement)this["Ping"]; }
        }
    }
}
