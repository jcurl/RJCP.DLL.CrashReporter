namespace RJCP.Diagnostics.Config
{
    using System.Configuration;

    internal sealed class WatchdogOverride : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Task
        {
            get { return (string)this["name"]; }
        }

        [ConfigurationProperty("warning", IsRequired = true)]
        public int WarningTimeout
        {
            get { return (int)this["warning"]; }
        }

        [ConfigurationProperty("critical", IsRequired = true)]
        public int CriticalTimeout
        {
            get { return (int)this["critical"]; }
        }
    }
}
