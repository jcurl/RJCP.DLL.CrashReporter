namespace RJCP.Diagnostics.Crash.Dumpers.NetVersion
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.Versioning;
    using System.Security;
    using Microsoft.Win32;
    using RJCP.Core.Environment;

    internal sealed class NetVersions : IEnumerable<INetVersion>
    {
        private readonly List<INetVersion> m_Installed = new();

        public NetVersions()
        {
            if (Platform.IsWinNT()) {
                FindNetFxVersions();
            }
        }

        [SupportedOSPlatform("windows")]
        private void FindNetFxVersions()
        {
            FindNetFx10();
            FindNetFxLegacy();

            try {
                NetFx.NetFx45 version45 = new();
                if (version45.IsValid) m_Installed.Add(version45);
            } catch (SecurityException) {
                /* Ignore */
            }
        }

        [SupportedOSPlatform("windows")]
        private void FindNetFx10()
        {
            try {
                using (RegistryKey netKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\Full\")) {
                    if (netKey is null) return;
                    foreach (string versionKeyName in netKey.GetSubKeyNames()) {
                        if (versionKeyName.StartsWith("v")) {
                            RegistryKey versionKey = netKey.OpenSubKey(versionKeyName);
                            foreach (string langCode in versionKey.GetSubKeyNames()) {
                                RegistryKey langKey = versionKey.OpenSubKey(langCode);
                                foreach (string product in langKey.GetSubKeyNames()) {
                                    RegistryKey productKey = langKey.OpenSubKey(product);
                                    if (IsInstalled(productKey)) {
                                        NetFx.NetFx10 net10 = new(versionKeyName, product);
                                        if (net10.IsValid) m_Installed.Add(net10);
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (SecurityException) {
                /* Ignore */
            }
        }

        // See https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed
        [SupportedOSPlatform("windows")]
        private void FindNetFxLegacy()
        {
            string netKey;
            if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess) {
                netKey = @"SOFTWARE\Wow6432Node\Microsoft\NET Framework Setup\NDP\";
            } else {
                netKey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\";
            }

            try {
                using (RegistryKey ndpKey = Registry.LocalMachine.OpenSubKey(netKey)) {
                    if (ndpKey is null) return;
                    foreach (string versionKeyName in ndpKey.GetSubKeyNames()) {
                        if (versionKeyName.StartsWith("v")) {
                            RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);

                            string defaultValue = versionKey.GetValue(null, "").ToString();
                            if (defaultValue is not null && defaultValue.Equals("deprecated")) return;

                            if (IsInstalled(versionKey)) {
                                NetFx.NetFxLegacy netfx = new(versionKeyName);
                                if (netfx.IsValid) m_Installed.Add(netfx);
                                continue;
                            }
                            foreach (string subKeyName in versionKey.GetSubKeyNames()) {
                                RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                                if (IsInstalled(subKey)) {
                                    string path = string.Format(@"{0}\{1}", versionKeyName, subKeyName);
                                    NetFx.NetFxLegacy netfx = new(path);
                                    if (netfx.IsValid) {
                                        m_Installed.Add(netfx);
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (SecurityException) {
                /* Ignore */
            }
        }

        [SupportedOSPlatform("windows")]
        private static bool IsInstalled(RegistryKey key)
        {
            string install = key.GetValue("Install", "").ToString();
            return install.Equals("1");
        }

        public IEnumerator<INetVersion> GetEnumerator()
        {
            return m_Installed.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
