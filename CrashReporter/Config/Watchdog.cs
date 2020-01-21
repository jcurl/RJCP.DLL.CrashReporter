namespace RJCP.Diagnostics.Config
{
    using System.Configuration;

    internal class Watchdog : ConfigurationSection
    {
        [ConfigurationProperty("Overrides", IsRequired = false, IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(WatchdogOverrides))]
        public WatchdogOverrides Overrides
        {
            get { return (WatchdogOverrides)this["Overrides"]; }
        }
    }
}
