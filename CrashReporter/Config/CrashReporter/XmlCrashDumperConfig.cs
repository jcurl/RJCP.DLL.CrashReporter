namespace RJCP.Diagnostics.Config.CrashReporter
{
    /// <summary>
    /// XmlCrashDumper Configuration.
    /// </summary>
    public class XmlCrashDumperConfig
    {
        private XmlCrashDumper m_Config;

        internal XmlCrashDumperConfig() { }

        internal XmlCrashDumperConfig(XmlCrashDumper config)
        {
            m_Config = config;
        }

        /// <summary>
        /// Gets the name of the style sheet resource in the user application.
        /// </summary>
        /// <value>The name of the style sheet resource. Returns <see cref="string.Empty"/> if not defined.</value>
        public string StyleSheetName { get { return m_Config?.StyleSheet.Name ?? string.Empty; } }
    }
}
