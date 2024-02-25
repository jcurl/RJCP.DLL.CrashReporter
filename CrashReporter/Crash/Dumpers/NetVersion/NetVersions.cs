namespace RJCP.Diagnostics.Crash.Dumpers.NetVersion
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.Versioning;
    using Microsoft.Win32;
    using RJCP.Core.Environment;
    using Runtime;

    /// <summary>
    /// A class to get the versions of .NET installed on the local computer. Only relevant for Windows.
    /// </summary>
    internal sealed class NetVersions : IEnumerable<INetVersion>
    {
        private readonly List<INetVersion> m_Installed = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="NetVersions"/> class.
        /// </summary>
        /// <remarks>
        /// Detects all installed versions of .NET Framework on a Windows platform.
        /// </remarks>
        public NetVersions()
        {
            if (Platform.IsWinNT()) {
                m_Installed.AddRange(NetFx.NetFx10.FindNetFx10());
                m_Installed.AddRange(NetFx.NetFxLegacy.FindNetFxLegacy());
                m_Installed.AddRange(NetFx.NetFx45.FindNetFx());
                m_Installed.AddRange(NetFx.Mono.FindMonoWindows());
            } else if (Platform.IsUnix()) {
                m_Installed.AddRange(NetFx.Mono.FindMonoLinux());
            }

            // Else the list is empty, if not Linux or Windows.
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection of .NET versions found.</returns>
        public IEnumerator<INetVersion> GetEnumerator()
        {
            return m_Installed.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static readonly object RunTimeLock = new();
        private static INetVersion s_RunTime;

        /// <summary>
        /// Gets the detected .NET runtime.
        /// </summary>
        /// <value>The current .NET runtime.</value>
        public static INetVersion Runtime
        {
            get
            {
                if (s_RunTime is null) {
                    lock (RunTimeLock) {
                        if (s_RunTime is null) {
                            INetVersion mono = new MonoRuntime();
                            if (mono.IsValid) {
                                s_RunTime = mono;
                            } else {
                                s_RunTime = new NetRuntime();
                            }
                        }
                    }
                }
                return s_RunTime;
            }
        }

        #region Internal common methods
        /// <summary>
        /// Determines whether the specified NET Framework is installed.
        /// </summary>
        /// <param name="key">The key for the framework.</param>
        /// <returns>
        /// <see langword="true"/> if the specified key is installed; otherwise, <see langword="false"/>.
        /// </returns>
        [SupportedOSPlatform("windows")]
        internal static bool IsInstalled(RegistryKey key)
        {
            return IsInstalled(key, "Install");
        }

        /// <summary>
        /// Determines whether the specified NET Framework is installed.
        /// </summary>
        /// <param name="key">The key for the framework.</param>
        /// <param name="value">The key entry (e.g. <c>Install</c>) to check.</param>
        /// <returns>
        /// <see langword="true"/> if the specified key is installed; otherwise, <see langword="false"/>.
        /// </returns>
        [SupportedOSPlatform("windows")]
        internal static bool IsInstalled(RegistryKey key, string value)
        {
            if (key is null) return false;
            string install = key.GetValue(value, "").ToString();
            return install.Equals("1", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Gets the version of the framework from the string.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <returns>The version of the framework, or <see langword="null"/> if it is invalid.</returns>
        internal static Version GetVersion(string version)
        {
            if (version.Length > 0 && (version[0] == 'v' || version[0] == 'V')) {
#if NETFRAMEWORK
                version = version.Substring(1);
#else
                version = version[1..];
#endif
            }

            if (Version.TryParse(version, out Version parsedVersion)) {
                return parsedVersion;
            }
            if (int.TryParse(version, out int parsedVersionSimple)) {
                return new Version(parsedVersionSimple, 0);
            }
            return null;
        }

        /// <summary>
        /// Gets the registry key, taking into account Windows on Windows.
        /// </summary>
        /// <param name="path">The path to append to <c>HKLM\Software\Microsoft</c>.</param>
        /// <returns>The formatted path for the registry.</returns>
        [SupportedOSPlatform("windows")]
        internal static string GetNetKey(string path)
        {
            if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess) {
                return $@"SOFTWARE\Wow6432Node\Microsoft\{path}";
            } else {
                return $@"SOFTWARE\Microsoft\{path}";
            }
        }

        /// <summary>
        /// Check if the string is a valid version key string in the Windows registry.
        /// </summary>
        /// <param name="version">The string key to check</param>
        /// <returns>
        /// <see langword="true"/> if this might be a valid version, <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// Note, this version doesn't do a complete check, but just enough if processing should continue.
        /// </remarks>
        internal static bool IsValidVersion(string version)
        {
            return version.Length > 1 && (version[0] == 'v' || version[0] == 'V');
        }
        #endregion
    }
}
