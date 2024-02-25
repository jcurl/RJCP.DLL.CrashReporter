namespace RJCP.Diagnostics.Crash.Dumpers.NetVersion.NetFx
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Versioning;
    using System.Security;
    using Microsoft.Win32;

    /// <summary>
    /// Installation details for .NET 1.0.
    /// </summary>
    internal sealed class NetFx10 : INetVersion
    {
        [SupportedOSPlatform("windows")]
        internal static IList<INetVersion> FindNetFx10()
        {
            List<INetVersion> installed = new();

            try {
                string netKeyName = NetVersions.GetNetKey(@"NET Framework Setup\Full\");
                using (RegistryKey netKey = Registry.LocalMachine.OpenSubKey(netKeyName)) {
                    if (netKey is null) return installed;
                    foreach (string versionKeyName in netKey.GetSubKeyNames()) {
                        if (NetVersions.IsValidVersion(versionKeyName)) {
                            using (RegistryKey versionKey = netKey.OpenSubKey(versionKeyName)) {
                                if (versionKey is null) continue;
                                foreach (string langCode in versionKey.GetSubKeyNames()) {
                                    using (RegistryKey langKey = versionKey.OpenSubKey(langCode)) {
                                        if (langKey is null) continue;
                                        foreach (string product in langKey.GetSubKeyNames()) {
                                            using (RegistryKey productKey = langKey.OpenSubKey(product)) {
                                                if (productKey is null) continue;
                                                if (NetVersions.IsInstalled(productKey)) {
                                                    NetFx10 net10 = new(versionKeyName, product);
                                                    if (net10.IsValid) installed.Add(net10);
                                                }
                                            }
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
        private NetFx10(string version, string key)
        {
            FrameworkVersion = NetVersions.GetVersion(version);
            if (FrameworkVersion is null) return;

            string fullKeyPath = NetVersions.GetNetKey($@"NET Framework Setup\Product\{key}");
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(fullKeyPath)) {
                string netRegistryVersion = registryKey.GetValue("Version", "").ToString();
                InstallVersion = NetVersions.GetVersion(netRegistryVersion);
                Package = registryKey.GetValue("Package", "").ToString();
                Description = $".NET Framework {netRegistryVersion} {Package}";
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
        /// Gets the package for .NET 1.0 as read from the registry.
        /// </summary>
        /// <value>The package as read from the registry.</value>
        public string Package { get; private set; }
    }
}
