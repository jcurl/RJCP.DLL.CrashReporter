namespace RJCP.Diagnostics.Config
{
    using System.Configuration;

    internal sealed class WatchdogPingElement : ConfigurationElement
    {
        [ConfigurationProperty("stack", DefaultValue = true, IsRequired = false)]
        public bool StackCapture
        {
            get { return (bool)this["stack"]; }
        }
    }
}
