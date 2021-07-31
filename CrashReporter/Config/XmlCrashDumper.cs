namespace RJCP.Diagnostics.Config
{
    using System.Configuration;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "There isn't a strict relationship")]
    internal class XmlCrashDumper : ConfigurationSection
    {
        [ConfigurationProperty("StyleSheet", IsRequired = false)]
        public StyleSheetElement StyleSheet
        {
            get { return (StyleSheetElement)this["StyleSheet"]; }
        }
    }
}
