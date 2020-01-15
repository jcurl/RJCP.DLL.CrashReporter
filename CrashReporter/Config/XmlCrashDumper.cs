namespace RJCP.Diagnostics.Config
{
    using System.Configuration;

    internal class XmlCrashDumper : ConfigurationSection
    {
        [ConfigurationProperty("StyleSheet", IsRequired = false)]
        public StyleSheetElement StyleSheet
        {
            get { return (StyleSheetElement)this["StyleSheet"]; }
        }
    }
}
