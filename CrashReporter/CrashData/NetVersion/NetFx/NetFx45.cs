namespace RJCP.Diagnostics.CrashData.NetVersion.NetFx
{
    using System;
    using System.Security;
    using Microsoft.Win32;
    using Net45;

    // See https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed
    internal class NetFx45 : INetVersion
    {
        internal NetFx45()
        {
            GetNetFx45Details();
        }

        internal NetFx45(int netfx45release)
        {
            GetNetVersion(netfx45release);
        }

        private void GetNetFx45Details()
        {
            try {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full")) {
                    if (key == null) {
                        IsValid = false;
                        return;
                    }

                    object objRelease = key.GetValue("Release");
                    if (objRelease == null) return;
                    GetNetVersion((int)objRelease);

                    object objTargetVersion = key.GetValue("TargetVersion");
                    if (objTargetVersion != null) {
                        TargetVersion = new Version(objTargetVersion as string);
                    }

                    object objNetVersion = key.GetValue("Version");
                    if (objNetVersion != null) {
                        NetVersion = new Version(objNetVersion as string);
                    }

                    if (Version == null) {
                        Version = NetVersion.ToString();
                        Description = string.Format(".NET Framework Net Version {0}, Release {1}", NetVersion, Net45Release);
                    }

                    IsValid = true;
                }
            } catch (SecurityException) {
                IsValid = false;
            } catch (Exception) {
                IsValid = false;
            }
        }

        private void GetNetVersion(int release)
        {
            Net45Release = release;

            NetFxVersion details = NetFxConfig.GetNetFxVersion(release);
            if (details == null) return;
            Version = details.Version;
            Description = details.Description;
        }

        public int Net45Release { get; private set; }

        public Version TargetVersion { get; private set; }

        public Version NetVersion { get; private set; }

        public bool IsValid { get; private set; }

        public string Version { get; private set; }

        public string Description { get; private set; }
    }
}
