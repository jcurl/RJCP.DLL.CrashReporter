namespace RJCP.Diagnostics.Config.CrashReporter
{
    using System;
    using System.Configuration;

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
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConfigurationSectionGroup grp;
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
                            XmlCrashDumper = new XmlCrashDumper((Config.XmlCrashDumper)grp.Sections["XmlCrashDumper"]);
                            break;
                        }
                    } catch (ConfigurationException) {
                        // Ignore exception errors. They'll be logged by default in the FirstChance.
                    }
                }
            }
        }

        public XmlCrashDumper XmlCrashDumper { get; private set; } = new XmlCrashDumper();
    }
}
