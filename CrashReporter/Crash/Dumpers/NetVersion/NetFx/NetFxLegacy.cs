namespace RJCP.Diagnostics.Crash.Dumpers.NetVersion.NetFx
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Text;
    using Microsoft.Win32;

    /// <summary>
    /// Older version of .NET 2.0 to 4.0.
    /// </summary>
    internal sealed class NetFxLegacy : INetVersion
    {
        [SupportedOSPlatform("windows")]
        internal static IList<INetVersion> FindNetFxLegacy()
        {
            List<INetVersion> installed = new();

            string netKey = NetVersions.GetNetKey(@"NET Framework Setup\NDP\");
            try {
                using (RegistryKey ndpKey = Registry.LocalMachine.OpenSubKey(netKey)) {
                    if (ndpKey is null) return installed;
                    foreach (string versionKeyName in ndpKey.GetSubKeyNames()) {
                        if (NetVersions.IsValidVersion(versionKeyName)) {
                            using (RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName)) {
                                if (versionKey is null) continue;
                                string defaultValue = versionKey.GetValue(null, "").ToString();
                                if (defaultValue is not null &&
                                    defaultValue.Equals("deprecated", StringComparison.InvariantCultureIgnoreCase)) continue;

                                if (NetVersions.IsInstalled(versionKey)) {
                                    NetFxLegacy netfx = new(versionKeyName);
                                    if (netfx.IsValid) installed.Add(netfx);
                                    continue;
                                }

                                // For .NET 3.0, we need to look into Setup
                                using (RegistryKey setup = versionKey.OpenSubKey("Setup")) {
                                    if (NetVersions.IsInstalled(setup, "InstallSuccess")) {
                                        NetFxLegacy netfx = new(versionKeyName, "Setup", true);
                                        if (netfx.IsValid) installed.Add(netfx);
                                        continue;
                                    }
                                }

                                // For .NET 4.0, this covers the "Client" and "Full" profile.
                                foreach (string subKeyName in new[] { "Client", "Full" }) {
                                    using (RegistryKey subKey = versionKey.OpenSubKey(subKeyName)) {
                                        if (subKey is null) continue;
                                        if (NetVersions.IsInstalled(subKey)) {
                                            NetFxLegacy netfx = new(versionKeyName, subKeyName);
                                            if (netfx.IsValid) installed.Add(netfx);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (SecurityException) {
                /* Ignore */
            }

            return installed;
        }

        [SupportedOSPlatform("windows")]
        private NetFxLegacy(string key) : this(key, null, true) { }

        [SupportedOSPlatform("windows")]
        private NetFxLegacy(string key, string profile) : this(key, profile, false) { }

        [SupportedOSPlatform("windows")]
        private NetFxLegacy(string key, string profile, bool ignoreProfile)
        {
            FrameworkVersion = NetVersions.GetVersion(key);
            if (FrameworkVersion is null) return;

            try {
                GetNetFxDetails(key, profile, ignoreProfile);
            } catch (SecurityException) {
                IsValid = false;
            }
        }

        [SupportedOSPlatform("windows")]
        private void GetNetFxDetails(string key, string profile, bool ignoreProfile)
        {
            string fullKeyPath;
            if (profile is null) {
                fullKeyPath = NetVersions.GetNetKey($@"NET Framework Setup\NDP\{key}");
            } else {
                fullKeyPath = NetVersions.GetNetKey($@"NET Framework Setup\NDP\{key}\{profile}");
            }

            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(fullKeyPath)) {
                if (registryKey is null) return;

                string installVersion = (string)registryKey.GetValue("Version");
                string objIncrement = (string)registryKey.GetValue("Increment");
                if (!int.TryParse(objIncrement, out int rev)) rev = -1;

                if (installVersion is null) {
                    if (rev > 0 && FrameworkVersion.Revision <= 0) {
                        InstallVersion = new Version(FrameworkVersion.Major, FrameworkVersion.Minor, FrameworkVersion.Build, rev);
                    } else {
                        InstallVersion = FrameworkVersion;
                    }
                } else {
                    Version version = NetVersions.GetVersion(installVersion);
                    if (rev > 0 && version.Revision <= 0) {
                        InstallVersion = new Version(version.Major, version.Minor, version.Build, rev);
                    } else {
                        InstallVersion = version;
                    }
                }

                Profile = ignoreProfile ? string.Empty : profile;
                string servicePack = registryKey.GetValue("SP", "").ToString();
                if (servicePack.Equals("0")) servicePack = string.Empty;

                StringBuilder description = new();
                description.Append(".NET Framework v").Append(FrameworkVersion.ToString());
                if (!string.IsNullOrEmpty(Profile))
                    description.Append(" Profile ").Append(profile);
                if (!string.IsNullOrEmpty(servicePack))
                    description.Append(" SP").Append(servicePack);
                Description = description.ToString();
                IsValid = true;
            }
        }

        /// <summary>
        /// Returns <see langword="true"/> if the version information contains valid (even if partially) information.
        /// </summary>
        /// <value><see langword="true"/> if this instance is valid; otherwise, <see langword="false"/>.</value>
        public bool IsValid { get; private set; }

        /// <summary>
        /// The .NET Version Type.
        /// </summary>
        public DotNetVersionType VersionType { get { return DotNetVersionType.NetFx; } }

        /// <summary>
        /// Gets the version that can be used for comparison.
        /// </summary>
        /// <value>The .NET version that can be used for comparison.</value>
        public Version FrameworkVersion { get; private set; }

        /// <summary>
        /// Gets the version of the installation.
        /// </summary>
        /// <value>The .NET installation version.</value>
        public Version InstallVersion { get; private set; }

        /// <summary>
        /// Gets the description of the .NET version installed.
        /// </summary>
        /// <value>The .NET version description.</value>
        public string Description { get; private set; }

        /// <summary>
        /// Defines the profile for use.
        /// </summary>
        /// <value>The profile for use.</value>
        public string Profile { get; private set; }
    }
}
