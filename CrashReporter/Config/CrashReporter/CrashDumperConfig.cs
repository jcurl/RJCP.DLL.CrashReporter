namespace RJCP.Diagnostics.Config.CrashReporter
{
    /// <summary>
    /// CrashDumper Configuration.
    /// </summary>
    public class CrashDumperConfig
    {
        internal CrashDumperConfig()
        {
            DumpDir = new DumpDir();
        }

        internal CrashDumperConfig(CrashDumper config)
        {
            DumpDir = new DumpDir(config.DumpDir);
        }

        /// <summary>
        /// Gets configuration properties for the dump directory.
        /// </summary>
        /// <value>The dump directory configuration properties.</value>
        public DumpDir DumpDir { get; private set; }
    }
}
