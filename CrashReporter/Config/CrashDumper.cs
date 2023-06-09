namespace RJCP.Diagnostics.Config
{
    using System.Configuration;

    internal sealed class CrashDumper : ConfigurationSection
    {
        [ConfigurationProperty("DumpDirectory", IsRequired = false)]
        public DumpDirElement DumpDir
        {
            get { return (DumpDirElement)this["DumpDirectory"]; }
        }
    }
}
