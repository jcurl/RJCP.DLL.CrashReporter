namespace RJCP.Diagnostics.CrashData.OSVersion
{
    using System;
    using Native.Win32;
#if DEBUG
    using System.Runtime.InteropServices;
#endif

    internal class OSVersion
    {
        private bool m_NativeSystemInfo;

        public OSVersion()
        {
            bool result = GetVersionEx();
            if (!result) GetVersion();
            GetSystemInfo();
            DetectArchitecture();
            GetProductInfo();
            DetectWin2003R2();
            DetectWinXP();
            DetectWinXPx64();
        }

        public OSPlatformId PlatformId { get; private set; }

        public Version Version { get; private set; }

        public Version ServicePack { get; private set; }

        public string CsdVersion { get; private set; }

        public OSSuites SuiteFlags { get; private set; }

        public OSProductType ProductType { get; private set; }

        public bool HasExtendedProperties { get; private set; }

        public OSProductInfo ProductInfo { get; private set; }

        public OSArchitecture Architecture { get; private set; }

        public bool ServerR2 { get; private set; }

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
            uint version = Kernel32.GetVersion();

            int majorVersion = (int)(version & 0xFF);
            int minorVersion = (int)((version & 0xFF00) >> 8);
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
            // We try to determine if we're a WOW64 process if we don't know the architecture
            // or if we're x86 and NativeSystemInfo didn't work.
            bool wow64 = false;
            bool result;
            try {
                result = Kernel32.IsWow64Process(Kernel32.GetCurrentProcess(), ref wow64);
            } catch (EntryPointNotFoundException) {
                result = false;
            }

            if (result) {
                if (wow64) {
                    // wow64 == true: 32-bit process on 64-bit windows.
                    Architecture = OSArchitecture.x86_x64;

                    // else:
                    //   wow64 == false: 32-bit on 32-bit; or 64-bit on 64-bit
                }
            }
        }

        private void GetProductInfo()
        {
            if (!HasExtendedProperties) return;

            uint productInfo = 0;
            bool result = false;
            try {
                result = Kernel32.GetProductInfo((uint)Version.Major, (uint)Version.Minor,
                    (uint)ServicePack.Major, (uint)ServicePack.Minor, ref productInfo);
            } catch (EntryPointNotFoundException) {
                // The operating system doesn't support this function call
            }

            if (!result) {
                ProductInfo = OSProductInfo.Undefined;
            } else {
                ProductInfo = (OSProductInfo)productInfo;
            }
        }

        private void GetSystemInfo()
        {
            Kernel32.SYSTEM_INFO lpSystemInfo = new Kernel32.SYSTEM_INFO();

            // GetNativeSystemInfo is independent if we're 64-bit or not
            // But it needs _WIN32_WINNT 0x0501
            try {
                Kernel32.GetNativeSystemInfo(ref lpSystemInfo);
                Architecture = (OSArchitecture)lpSystemInfo.uProcessorInfo.wProcessorArchitecture;
                m_NativeSystemInfo = true;
            } catch {
                Architecture = OSArchitecture.Unknown;
                m_NativeSystemInfo = false;
            }

            if (Architecture == OSArchitecture.Unknown || !m_NativeSystemInfo) {
                try {
                    Kernel32.GetSystemInfo(ref lpSystemInfo);
                    Architecture = (OSArchitecture)lpSystemInfo.uProcessorInfo.wProcessorArchitecture;
                } catch {
                    Architecture = OSArchitecture.Unknown;
                }
            }
        }

        private void DetectWin2003R2()
        {
            if (Version.Major == 5 && Version.Minor == 2) {
                ServerR2 = (User32.GetSystemMetrics(User32.SystemMetrics.SM_SERVERR2) != 0);
            }
        }

        private void DetectWinXP()
        {
            int result;

            if (Version.Major == 5 && Version.Minor == 1) {
                ProductInfo = OSProductInfo.Undefined;

                result = User32.GetSystemMetrics(User32.SystemMetrics.SM_MEDIACENTER);
                if (result != 0) ProductInfo = OSProductInfo.MediaCenter;

                result = User32.GetSystemMetrics(User32.SystemMetrics.SM_TABLETPC);
                if (result != 0) ProductInfo = OSProductInfo.TabletPc;

                result = User32.GetSystemMetrics(User32.SystemMetrics.SM_STARTER);
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
    }
}
