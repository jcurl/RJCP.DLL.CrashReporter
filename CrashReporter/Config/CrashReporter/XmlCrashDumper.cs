namespace RJCP.Diagnostics.Config.CrashReporter
{
    internal class XmlCrashDumper
    {
        private Config.XmlCrashDumper m_Config;

        public XmlCrashDumper() { }

        public XmlCrashDumper(Config.XmlCrashDumper config)
        {
            m_Config = config;
        }

        public string StyleSheetName { get { return m_Config?.StyleSheet.Name ?? string.Empty; } }
    }
}
