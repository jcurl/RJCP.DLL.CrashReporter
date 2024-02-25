namespace RJCP.Diagnostics.Crash.Dumpers.NetVersion.NetFx
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Versioning;
    using System.Security;
    using Microsoft.Win32;
    using Net45;

    /// <summary>
    /// Installation details for .NET 4.5 and later (up to .NET 4.8).
    /// </summary>
    internal sealed class NetFx45 : INetVersion
    {
        [SupportedOSPlatform("windows")]
        internal static IList<INetVersion> FindNetFx()
        {
            List<INetVersion> installed = new();

            try {
                NetFx45 version45 = new();
                if (version45.IsValid) installed.Add(version45);
            } catch (SecurityException) {
                /* Ignore */
            }

            return installed;
        }

        [SupportedOSPlatform("windows")]
        private NetFx45()
        {
            try {
                string fullKeyPath = NetVersions.GetNetKey(@"NET Framework Setup\NDP\v4\Full");
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(fullKeyPath)) {
                    if (key is null) return;

                    object objRelease = key.GetValue("Release");
                    if (objRelease is null) return;
                    Net45Release = (int)objRelease;
                    NetFxVersion details = NetFxConfig.GetNetFxVersion(Net45Release);
                    FrameworkVersion = details?.Version;

                    object objTargetVersion = key.GetValue("TargetVersion");
                    if (objTargetVersion is not null) {
                        TargetVersion = NetVersions.GetVersion(objTargetVersion as string);
                    }

                    object objInstallVersion = key.GetValue("Version");
                    if (objInstallVersion is not null) {
                        InstallVersion = NetVersions.GetVersion(objInstallVersion as string);
                    }

                    Description = string.Format(".NET Framework v{0}, Release {1} ({2})",
                        FrameworkVersion?.ToString() ?? string.Empty, Net45Release,
                        details?.Description ?? ".NET Framework 4.x");

                    IsValid = true;
                }
            } catch (SecurityException) {
                IsValid = false;
            } catch (Exception) {
                IsValid = false;
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
        /// Gets the release version of .NET 4.5 and later.
        /// </summary>
        /// <value>The release version of .NET 4.5 and later.</value>
        public int Net45Release { get; private set; }

        /// <summary>
        /// Gets the target version for the runtime.
        /// </summary>
        /// <value>The target version for the runtime.</value>
        public Version TargetVersion { get; private set; }
    }
}
