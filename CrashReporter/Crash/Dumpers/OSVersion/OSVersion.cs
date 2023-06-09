namespace RJCP.Diagnostics.Crash.Dumpers.OSVersion
{
    using System;
    using Native.Win32;
    using Microsoft.Win32;

#if DEBUG
    using System.Runtime.InteropServices;
#endif

    internal class OSVersion
    {
        public OSVersion()
        {
            // Ignore exceptions in the log related to the entry point not found.
            using (CrashReporter.SuppressFirstChanceException()) {
                bool result = GetVersionEx();
                if (!result) GetVersion();
                DetectArchitecture();
                GetProductInfo();
                DetectWin2003R2();
                DetectWinXP();
                DetectWinXPx64();
                DetectWinBrand();
                DetectWin10();
            }
        }

        public OSPlatformId PlatformId { get; private set; }

        public Version Version { get; private set; }

        public Version ServicePack { get; private set; }

        public string CsdVersion { get; private set; }

        public OSSuites SuiteFlags { get; private set; }

        public OSProductType ProductType { get; private set; }

        public bool HasExtendedProperties { get; private set; }

        public OSProductInfo ProductInfo { get; private set; }

        public string Architecture { get; private set; }

        public string NativeArchitecture { get; private set; }

        public bool ServerR2 { get; private set; }

        public string ReleaseInfo { get; private set; } = string.Empty;

        private bool GetVersionEx()
        {
            bool result = false;

            // Get the basic information. If this shows we've got a newer operating
            // system, we can get more detailed information later.
            Kernel32.OSVERSIONINFO info = new Kernel32.OSVERSIONINFO();
            try {
                result = Kernel32.GetVersionEx(info);
                if (!result) {
#if DEBUG
                    int error = Marshal.GetLastWin32Error();
                    string message = string.Format("OSVersion.GetVersionEx: Returned error {0}", error);
                    System.Diagnostics.Trace.WriteLine(message);
#endif
                    return false;
                }
            } catch (EntryPointNotFoundException) {
                return false;
            }

            PlatformId = (OSPlatformId)info.PlatformId;
            CsdVersion = info.CSDVersion;
            if (PlatformId == OSPlatformId.WinNT) {
                if (info.MajorVersion < 4) {
                    // Windows 3.51 or earlier
                    Version = new Version(info.MajorVersion, info.MinorVersion, info.BuildNumber);
                    return true;
                } else if (info.MajorVersion == 4) {
                    if (info.MinorVersion == 0) {
                        if (!CsdVersion.Equals("Service Pack 6", StringComparison.Ordinal)) {
                            // Earlier than WinNT 4.0 SP6
                            Version = new Version(info.MajorVersion, info.MinorVersion, info.BuildNumber);
                            return true;
                        }
                    }
                }
            } else if (PlatformId == OSPlatformId.Win9x) {
                Version = new Version(info.MajorVersion, info.MinorVersion, info.BuildNumber & 0xFFFF);
                return true;
            }

            Kernel32.OSVERSIONINFOEX infoex = new Kernel32.OSVERSIONINFOEX();
            result = Kernel32.GetVersionEx(infoex);
            if (!result) {
#if DEBUG
                int error = Marshal.GetLastWin32Error();
                string message = string.Format("OSVersion.GetVersionEx(infoex): Returned error {0}", error);
                System.Diagnostics.Trace.WriteLine(message);
#endif
                return true;
            }

            int ntstatus = -1;
            Kernel32.OSVERSIONINFOEX rtlInfoEx = new Kernel32.OSVERSIONINFOEX();
            try {
                ntstatus = NtDll.RtlGetVersion(rtlInfoEx);
            } catch (EntryPointNotFoundException) {
                // The RtlGetVersionEx() call doesn't exist, or it returned an error
                ntstatus = -1;
            }

            bool newer = false;
            Version vInfo = new Version(infoex.MajorVersion, infoex.MinorVersion, infoex.BuildNumber);
            Version vRtl = new Version(rtlInfoEx.MajorVersion, rtlInfoEx.MinorVersion, rtlInfoEx.BuildNumber);
            if (ntstatus == 0) {
                // The direct call worked, and should overcome the API breakage that
                // depends on a manifest. Just in case that the real method returns a
                // value that is older than the "real" windows API.
                newer = vRtl >= vInfo;
            } else {
                newer = false;
            }

            if (newer) {
                PlatformId = (OSPlatformId)rtlInfoEx.PlatformId;
                Version = vRtl;
                CsdVersion = rtlInfoEx.CSDVersion;
                SuiteFlags = (OSSuites)rtlInfoEx.SuiteMask;
                ProductType = (OSProductType)rtlInfoEx.ProductType;
                ServicePack = new Version(rtlInfoEx.ServicePackMajor, rtlInfoEx.ServicePackMinor);
            } else {
                PlatformId = (OSPlatformId)infoex.PlatformId;
                Version = vInfo;
                CsdVersion = infoex.CSDVersion;
                SuiteFlags = (OSSuites)infoex.SuiteMask;
                ProductType = (OSProductType)infoex.ProductType;
                ServicePack = new Version(infoex.ServicePackMajor, infoex.ServicePackMinor);
            }

            HasExtendedProperties = true;
            return true;
        }

        private void GetVersion()
        {
            int version = Kernel32.GetVersion();
            int majorVersion = version & 0xFF;
            int minorVersion = (version & 0xFF00) >> 8;
            int buildNumber = 0;

            if ((version & (1 << 31)) != 0) {
                // Win9x or Win32s
                if (majorVersion == 3) {
                    PlatformId = OSPlatformId.Win32s;
                } else {
                    PlatformId = OSPlatformId.Win9x;
                }
            } else {
                // WinNT
                buildNumber = (int)((version & 0x7FFF0000) >> 16);
                PlatformId = OSPlatformId.WinNT;
            }

            Version = new Version(majorVersion, minorVersion, buildNumber);
        }

        private void DetectArchitecture()
        {
            if (DetectArchitectureWithWow2())
                return;

            DetectArchitectureWithSystemInfo();
        }

        private bool DetectArchitectureWithWow2()
        {
            try {
                bool result = Kernel32.IsWow64Process2(Kernel32.GetCurrentProcess(),
                    out Kernel32.IMAGE_FILE_MACHINE processMachine, out Kernel32.IMAGE_FILE_MACHINE nativeMachine);
                if (!result) return false;

                NativeArchitecture = OSArchitecture.GetImageFileMachineString(nativeMachine);
                if (processMachine == Kernel32.IMAGE_FILE_MACHINE.UNKNOWN) {
                    // This is not a WoW process, so it's the same as the native architecture.
                    Architecture = NativeArchitecture;
                } else {
                    Architecture = OSArchitecture.GetImageFileMachineString(processMachine);
                }
                return true;
            } catch (EntryPointNotFoundException) {
                return false;
            }
        }

        private void DetectArchitectureWithSystemInfo()
        {
            Kernel32.SYSTEM_INFO lpSystemInfo;

            // GetNativeSystemInfo is independent if we're 64-bit or not But it needs _WIN32_WINNT 0x0501
            ushort processorNativeArchitecture;
            try {
                Kernel32.GetNativeSystemInfo(out lpSystemInfo);
                processorNativeArchitecture = lpSystemInfo.uProcessorInfo.wProcessorArchitecture;
            } catch (EntryPointNotFoundException) {
                processorNativeArchitecture = Kernel32.PROCESSOR_ARCHITECTURE.UNKNOWN;
            }

            if (processorNativeArchitecture == Kernel32.PROCESSOR_ARCHITECTURE.UNKNOWN) {
                Kernel32.GetSystemInfo(out lpSystemInfo);
                processorNativeArchitecture = lpSystemInfo.uProcessorInfo.wProcessorArchitecture;
            }

            NativeArchitecture = OSArchitecture.GetProcessArchitecture(processorNativeArchitecture);

            switch (processorNativeArchitecture) {
            case Kernel32.PROCESSOR_ARCHITECTURE.IA64:
            case Kernel32.PROCESSOR_ARCHITECTURE.AMD64:
                bool result = Kernel32.IsWow64Process(Kernel32.GetCurrentProcess(), out bool wow64);
                if (result && wow64) {
                    Architecture = OSArchitecture.GetProcessArchitecture(Kernel32.PROCESSOR_ARCHITECTURE.INTEL);
                }
                break;
            }

            if (Architecture == null)
                Architecture = NativeArchitecture;
        }

        private void GetProductInfo()
        {
            if (!HasExtendedProperties) return;

            try {
                bool result = Kernel32.GetProductInfo(Version.Major, Version.Minor,
                    ServicePack.Major, ServicePack.Minor, out OSProductInfo productInfo);
                if (!result) {
                    ProductInfo = OSProductInfo.Undefined;
                } else {
                    ProductInfo = productInfo;
                }
            } catch (EntryPointNotFoundException) {
                // The operating system doesn't support this function call (e.g. Windows XP)
            }
        }

        private void DetectWin2003R2()
        {
            if (Version.Major == 5 && Version.Minor == 2) {
                ServerR2 = (User32.GetSystemMetrics(User32.SYSTEM_METRICS.SM_SERVERR2) != 0);
            }
        }

        private void DetectWinXP()
        {
            int result;

            if (Version.Major == 5 && Version.Minor == 1) {
                ProductInfo = OSProductInfo.Undefined;

                result = User32.GetSystemMetrics(User32.SYSTEM_METRICS.SM_MEDIACENTER);
                if (result != 0) ProductInfo = OSProductInfo.MediaCenter;

                result = User32.GetSystemMetrics(User32.SYSTEM_METRICS.SM_TABLETPC);
                if (result != 0) ProductInfo = OSProductInfo.TabletPc;

                result = User32.GetSystemMetrics(User32.SYSTEM_METRICS.SM_STARTER);
                if (result != 0) ProductInfo = OSProductInfo.Starter;

                if (ProductInfo == OSProductInfo.Undefined) {
                    if ((SuiteFlags & OSSuites.Personal) != 0) {
                        ProductInfo = OSProductInfo.Home_Premium;
                    } else {
                        ProductInfo = OSProductInfo.Professional;
                    }
                }
            }
        }

        private void DetectWinXPx64()
        {
            if (Version.Major == 5 && Version.Minor == 2) {
                if (ProductType == OSProductType.Workstation) {
                    ProductInfo = OSProductInfo.Professional;
                }
            }
        }

        private void DetectWinBrand()
        {
            try {
                // Useful to tell the difference between Windows 10 and Windows 11.
                string release = WinBrand.BrandingFormatString("%WINDOWS_LONG%");
                if (!string.IsNullOrWhiteSpace(release)) ReleaseInfo = release;
            } catch (EntryPointNotFoundException) {
                // Ignore that the DLL or the entry point can't be found
            }
        }

        private void DetectWin10()
        {
            if (Version.Major < 10) return;

            try {
                RegistryKey currentVersion =
                    Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                if (currentVersion != null) {
                    object ubrobj = currentVersion.GetValue("UBR");
                    if (ubrobj is int ubr) {
                        Version = new Version(Version.Major, Version.Minor, Version.Build, ubr);
                    }

                    if (string.IsNullOrWhiteSpace(ReleaseInfo)) {
                        if (currentVersion.GetValue("DisplayVersion") is string releaseId &&
                            currentVersion.GetValue("ProductName") is string productName) {
                            ReleaseInfo = $"{productName} ({releaseId})";
                        }
                    }
                }
            } catch {
                // Ignore any errors (Security, registry errors, etc.).
            }
        }
    }
}
