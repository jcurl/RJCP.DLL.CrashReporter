namespace RJCP.Diagnostics.Config
{
    using System.Configuration;

    internal class CrashDumper : ConfigurationSection
    {
        [ConfigurationProperty("DumpDirectory", IsRequired = false)]
        public DumpDirElement DumpDir
        {
            get { return (DumpDirElement)this["DumpDirectory"]; }
        }
    }
}
