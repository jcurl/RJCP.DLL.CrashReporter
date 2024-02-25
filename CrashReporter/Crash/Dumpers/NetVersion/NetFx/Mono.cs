namespace RJCP.Diagnostics.Crash.Dumpers.NetVersion.NetFx
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Versioning;
    using System.Security;
    using Microsoft.Win32;
    using Runtime;

    /// <summary>
    /// Installation details for the Mono Runtime.
    /// </summary>
    internal sealed class Mono : INetVersionMono
    {
        private const string NovellKey = @"SOFTWARE\Novell\Mono";
        private const string NovellKey86 = @"SOFTWARE\Wow6432Node\Novell\Mono";
        private const string MonoKey = @"SOFTWARE\Mono";
        private const string MonoKey86 = @"SOFTWARE\Wow6432Node\Mono";

        /// <summary>
        /// Mono installation database.
        /// </summary>
        private sealed class MonoInstall
        {
            private readonly List<INetVersion> m_Installed = new();

            /// <summary>
            /// Gets the found mono installations.
            /// </summary>
            /// <value>
            /// The found installations of Mono (from <see cref="AddMonoProfile(string, string, string)"/>).
            /// </value>
            public IList<INetVersion> Installed
            {
                get { return m_Installed; }
            }

            /// <summary>
            /// Adds profiles from the Mono path, if this path hasn't already been detected.
            /// </summary>
            /// <param name="path">The path to the mono binary.</param>
            /// <param name="assemblyDir">The FrameworkAssemblyDirectory directory.</param>
            /// <param name="version">The version of the Mono installation.</param>
            /// <remarks>
            /// The architecture is determined from the binary. It must be either an ELF file, or a Windows PE file. It
            /// is queried for the architecture.
            /// </remarks>
            public bool AddMonoProfile(string path, string assemblyDir, string version)
            {
                return AddMonoProfile(path, assemblyDir, version, null);
            }

            /// <summary>
            /// Adds profiles from the Mono path, if this path hasn't already been detected.
            /// </summary>
            /// <param name="path">The path to the mono binary.</param>
            /// <param name="assemblyDir">The FrameworkAssemblyDirectory directory.</param>
            /// <param name="version">The version of the Mono installation.</param>
            /// <param name="arch">
            /// The architecture for the Mono implementation. Set to <see langword="null"/> for automatic detection
            /// based on the EXE file.
            /// </param>
            /// <returns>
            /// Returns <see langword="true"/> if the path is already known or added to the list. <see langword="false"/>
            /// otherwise, which can occur if the file is invalid, or the architecture is unknown.
            /// </returns>
            public bool AddMonoProfile(string path, string assemblyDir, string version, string arch)
            {
                if (!File.Exists(path)) return false;
                if (!Directory.Exists(assemblyDir)) return false;

                arch ??= string.Empty;

                Version net35 = null;
                Version net11 = GetFileVersion(assemblyDir, "mono", "1.0", "mscorlib.dll");
                Version net20 = GetFileVersion(assemblyDir, "mono", "2.0", "mscorlib.dll");
                Version net40 = GetFileVersion(assemblyDir, "mono", "4.0", "mscorlib.dll");
                Version net45 = GetFileVersion(assemblyDir, "mono", "4.5", "mscorlib.dll");
                if (net20 is not null) {
                    if (Directory.Exists(Path.Combine(assemblyDir, @"mono\3.5"))) {
                        net35 = net20;
                    }
                }

                if (!Version.TryParse(version, out Version installVersion))
                    installVersion = new Version(0, 0);

                if (net11 is not null) m_Installed.Add(new Mono(new Version(1, 1, 4322), net11, installVersion, path, arch));
                if (net20 is not null) m_Installed.Add(new Mono(new Version(2, 0), net20, installVersion, path, arch));
                if (net35 is not null) m_Installed.Add(new Mono(new Version(3, 5), net35, installVersion, path, arch));
                if (net40 is not null) m_Installed.Add(new Mono(new Version(4, 0), net40, installVersion, path, arch));
                if (net45 is not null) m_Installed.Add(new Mono(new Version(4, 5), net45, installVersion, path, arch));
                return true;
            }

            private static Version GetFileVersion(params string[] path)
            {
                string fullPath = Path.Combine(path);
                if (!File.Exists(fullPath)) return null;

                try {
                    FileVersionInfo info = FileVersionInfo.GetVersionInfo(fullPath);
                    if (Version.TryParse(info.FileVersion, out Version version))
                        return version;
                    return new Version(0, 0);
                } catch (FileNotFoundException) {
                    return null;
                }
            }
        }

        [SupportedOSPlatform("windows")]
        internal static IList<INetVersion> FindMonoWindows()
        {
            MonoInstall installed = new();

            // Check both 32-bit and 64-bit installations (if running on a 32-bit framework, can only check for the
            // 32-bit installation).
            FindMono4Windows(MonoKey, installed);
            if (Environment.Is64BitProcess) FindMono4Windows(MonoKey86, installed);

            // Check for earlier versions of Mono, 1.0 to 3.2.3 (which was the last version for Windows XP).
            if (installed.Installed.Count == 0) {
                // These versions only existed as 32-bit.
                if (Environment.Is64BitProcess) {
                    FindMonoNovellWindows(NovellKey86, installed);
                } else {
                    FindMonoNovellWindows(NovellKey, installed);
                }
            }

            // If a Modern Mono (4.2.3 or later) was not found, check explicitly also the Program Files, as versions
            // 3.12.0 - 4.0.3 didn't have any registry keys when installing.
            if (installed.Installed.Count == 0) {
                // Not running on Mono so we "guess" the path.
                if (Environment.Is64BitProcess)
                    FindMonoFromSdkPath(
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Mono"),
                        null, installed);
                FindMonoFromSdkPath(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Mono"),
                    null, installed);
            }

            // Always add the current flavour of Mono as an available runtime.
            string binPath = MonoRuntime.GetMonoRuntimeClrPath();
            FindMonoFromPath(binPath, installed);

            // Finally, search the path looking for the first copy of Mono
            string envPath = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrWhiteSpace(envPath)) {
                string[] paths = envPath.Split(';');
                foreach (string path in paths) {
                    string monoPath = Path.Combine(path, "mono.exe");
                    if (FindMonoFromPath(monoPath, installed)) break;
                }
            }

            return installed.Installed;
        }

        [SupportedOSPlatform("linux")]
        internal static IList<INetVersion> FindMonoLinux()
        {
            MonoInstall installed = new();

            // Always add the current flavour of Mono as an available runtime.
            string binPath = MonoRuntime.GetMonoRuntimeClrPath();
            FindMonoFromPath(binPath, installed);

            // Finally, search the path looking for the first copy of Mono
            string envPath = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrWhiteSpace(envPath)) {
                string[] paths = envPath.Split(':');
                foreach (string path in paths) {
                    string monoPath = Path.Combine(path, "mono");
                    if (FindMonoFromPath(monoPath, installed)) break;
                }
            }

            return installed.Installed;
        }

        [SupportedOSPlatform("windows")]
        private static void FindMono4Windows(string key, MonoInstall installed)
        {
            try {
                using (RegistryKey monoKey = Registry.LocalMachine.OpenSubKey(key)) {
                    if (monoKey is null) return;
                    if (!NetVersions.IsInstalled(monoKey, "Installed")) return;
                    if (monoKey.GetValue("SdkInstallRoot") is not string sdkDir) return;
                    string monoPath = Path.Combine(sdkDir, "bin", "mono.exe");
                    string assemblyDir = monoKey.GetValue("FrameworkAssemblyDirectory") as string;
                    string version = monoKey.GetValue("Version") as string;
                    string arch = monoKey.GetValue("Architecture") as string;
                    installed.AddMonoProfile(monoPath, assemblyDir, version, arch);
                }
            } catch (SecurityException) {
                /* Ignore security exception */
            }
        }

        [SupportedOSPlatform("windows")]
        private static void FindMonoNovellWindows(string key, MonoInstall installed)
        {
            try {
                using (RegistryKey monoKey = Registry.LocalMachine.OpenSubKey(key)) {
                    if (monoKey is null) return;
                    // If this key doesn't exist, then the user must reinstall (they uninstalled a version, which
                    // removed the key, even though other versions may still be present).
                    if (monoKey.GetValue("DefaultCLR") is string defaultClr) {
                        FindMonoNovellWindows(monoKey, defaultClr, installed);
                    }
                }
            } catch (SecurityException) {
                /* Ignore security exception */
            }
        }

        [SupportedOSPlatform("windows")]
        private static void FindMonoNovellWindows(RegistryKey monoKey, string version, MonoInstall installed)
        {
            try {
                using (RegistryKey installKey = monoKey.OpenSubKey(version)) {
                    if (installKey is null) return;
                    if (installKey.GetValue("SdkInstallRoot") is not string sdkDir) return;
                    string monoPath = Path.Combine(sdkDir, "bin", "mono.exe");
                    string assemblyDir = installKey.GetValue("FrameworkAssemblyDirectory") as string;
                    installed.AddMonoProfile(monoPath, assemblyDir, version, "x86");
                }
            } catch (SecurityException) {
                /* Ignore security exception */
            }
        }

        private static bool FindMonoFromPath(string monoPath, MonoInstall installed)
        {
            if (!File.Exists(monoPath)) return false;
            string sdkRoot = Path.GetDirectoryName(monoPath);
            sdkRoot = Path.GetDirectoryName(sdkRoot);
            return FindMonoFromSdkPath(sdkRoot, monoPath, installed);
        }

        private static bool FindMonoFromSdkPath(string sdkPath, string monoPath, MonoInstall installed)
        {
            monoPath ??= Path.Combine(sdkPath, "bin", "mono.exe");
            string assemblyDir = Path.Combine(sdkPath, "lib");
            return installed.AddMonoProfile(monoPath, assemblyDir, null);
        }

        private Mono(Version framework, Version file, Version install, string path, string arch)
        {
            if (framework is null) return;
            FrameworkVersion = framework;
            InstallVersion = install ?? file;
            MonoPath = path;
            MsCorLibVersion = file;
            Architecture = arch ?? "x86";
            IsValid = true;

            if (InstallVersion is not null) {
                Description = $".NET Framework v{FrameworkVersion}, Mono Runtime v{InstallVersion} {arch}";
            } else {
                Description = $".NET Framework v{FrameworkVersion}, Mono Runtime {arch}";
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
        public DotNetVersionType VersionType { get { return DotNetVersionType.Mono; } }

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
        /// Gets the path to the Mono binary.
        /// </summary>
        public string MonoPath { get; private set; }

        /// <summary>
        /// Gets the version of the MSCorLib.DLL file installed.
        /// </summary>
        public Version MsCorLibVersion { get; private set; }

        /// <summary>
        /// Gets the architecture for the Mono runtime.
        /// </summary>
        public string Architecture { get; private set; }
    }
}
