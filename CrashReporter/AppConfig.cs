namespace RJCP.Diagnostics
{
    using System;
    using System.Configuration;
    using System.Diagnostics;

    internal class AppConfig
    {
        private static readonly object s_SyncLock = new object();
        private static AppConfig s_Config;

        public static AppConfig Config
        {
            get
            {
                if (s_Config == null) {
                    lock (s_SyncLock) {
                        if (s_Config == null) {
                            s_Config = new AppConfig();
                        }
                    }
                }
                return s_Config;
            }
        }

        private AppConfig()
        {
            Configuration config;
            ConfigurationSectionGroup grp = null;
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
                                XmlCrashDumper = new Config.CrashReporter.XmlCrashDumperConfig((Config.XmlCrashDumper)grp.Sections["XmlCrashDumper"]);
                                break;
                            case "Watchdog":
                                Watchdog = new Config.CrashReporter.WatchdogConfig((Config.Watchdog)grp.Sections["Watchdog"]);
                                break;
                            }
                        } catch (ConfigurationException) {
                            // Ignore exception errors. They'll be logged by default in the FirstChance.
                        }
                    }
                }
            } catch (ConfigurationErrorsException ex) {
                // There was an error loading the configuration, so we use the default configuration
                Log.CrashLog.TraceEvent(TraceEventType.Warning, 0, "Error loading configuration: {0}", ex.Message);
            }

            // Provide default implementations is it is not in the configuration. Code doesn't need to check for 'null'
            // making it safer.
            if (XmlCrashDumper == null) XmlCrashDumper = new Config.CrashReporter.XmlCrashDumperConfig();
            if (Watchdog == null) Watchdog = new Config.CrashReporter.WatchdogConfig();
        }

        public Config.CrashReporter.XmlCrashDumperConfig XmlCrashDumper { get; private set; }

        public Config.CrashReporter.WatchdogConfig Watchdog { get; private set; }
    }
}
