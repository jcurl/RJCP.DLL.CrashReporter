namespace RJCP.Diagnostics.Config.CrashReporter
{
    using System;
    using System.Configuration;
    using System.Diagnostics;

    /// <summary>
    /// Exposes the application configuration information for the CrashReporter.
    /// </summary>
    public class CrashConfig
    {
        internal CrashConfig()
        {
            Configuration config;
            ConfigurationSectionGroup grp;
            try {
                config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                try {
                    grp = config.SectionGroups["CrashReporter"];
                } catch (ArgumentException) {
                    // The CrashReporter group is not defined, so ignore it. Although this is probably because the
                    // section <CrashReporter> is present, but not the <sectionGroup name="CrashReporter">. Observed
                    // that crashes occur somewhere else later anyway when single-stepping.
                    return;
                }

                if (grp != null) {
                    foreach (ConfigurationSection section in grp.Sections) {
                        try {
                            switch (section.SectionInformation.Name) {
                            case "XmlCrashDumper":
                                XmlCrashDumper = new XmlCrashDumperConfig((XmlCrashDumper)grp.Sections["XmlCrashDumper"]);
                                break;
                            case "Watchdog":
                                Watchdog = new WatchdogConfig((Watchdog)grp.Sections["Watchdog"]);
                                break;
                            case "CrashDumper":
                                CrashDumper = new CrashDumperConfig((CrashDumper)grp.Sections["CrashDumper"]);
                                break;
                            }
                        } catch (ConfigurationException) {
                            // Ignore exception errors. They'll be logged by default in the FirstChance.
                        }
                    }
                }
            } catch (ConfigurationErrorsException ex) {
                // There was an error loading the configuration, so we use the default configuration
                Log.CrashLog.TraceEvent(TraceEventType.Warning, "Error loading configuration: {0}", ex.Message);
            }

            // Provide default implementations is it is not in the configuration. Code doesn't need to check for 'null'
            // making it safer.
            if (XmlCrashDumper == null) XmlCrashDumper = new XmlCrashDumperConfig();
            if (Watchdog == null) Watchdog = new WatchdogConfig();
            if (CrashDumper == null) CrashDumper = new CrashDumperConfig();
        }

        /// <summary>
        /// Gets the XML crash dumper configuration if configured.
        /// </summary>
        /// <value>The XML crash dumper configuration.</value>
        public XmlCrashDumperConfig XmlCrashDumper { get; private set; }

        /// <summary>
        /// Gets the watchdog configuration.
        /// </summary>
        /// <value>The watchdog configuration.</value>
        public WatchdogConfig Watchdog { get; private set; }

        /// <summary>
        /// Gets the crash dumper configuration.
        /// </summary>
        /// <value>The crash dumper configuration.</value>
        public CrashDumperConfig CrashDumper { get; private set; }
    }
}
