namespace RJCP.Diagnostics.Config
{
    using System.Configuration;

    internal class DumpDirElement : ConfigurationElement
    {
        [ConfigurationProperty("path", DefaultValue = "", IsRequired = false)]
        public string Path
        {
            get { return (string)this["path"]; }
        }

        [ConfigurationProperty("ageDays", DefaultValue = "", IsRequired = false)]
        public string AgeDays
        {
            get { return (string)this["ageDays"]; }
        }

        [ConfigurationProperty("maxLogs", DefaultValue = "", IsRequired = false)]
        public string MaxLogs
        {
            get { return (string)this["maxLogs"]; }
        }

        [ConfigurationProperty("freeGb", DefaultValue = "", IsRequired = false)]
        public string ReserveFree
        {
            get { return (string)this["freeGb"]; }
        }

        [ConfigurationProperty("freePercent", DefaultValue = "", IsRequired = false)]
        public string ReserveFreePercent
        {
            get { return (string)this["freePercent"]; }
        }

        [ConfigurationProperty("maxGb", DefaultValue = "", IsRequired = false)]
        public string MaxDirSize
        {
            get { return (string)this["maxGb"]; }
        }

        [ConfigurationProperty("minLogs", DefaultValue = "", IsRequired = false)]
        public string MaxDirSizeMinLogs
        {
            get { return (string)this["minLogs"]; }
        }
    }
}
