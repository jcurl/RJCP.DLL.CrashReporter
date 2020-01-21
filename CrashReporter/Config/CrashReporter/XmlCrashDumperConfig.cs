namespace RJCP.Diagnostics.Config.CrashReporter
{
    internal class XmlCrashDumperConfig
    {
        private XmlCrashDumper m_Config;

        public XmlCrashDumperConfig() { }

        public XmlCrashDumperConfig(XmlCrashDumper config)
        {
            m_Config = config;
        }

        public string StyleSheetName { get { return m_Config?.StyleSheet.Name ?? string.Empty; } }
    }
}
