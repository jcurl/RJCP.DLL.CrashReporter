namespace RJCP.Diagnostics.Native.Win32
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using Crash.Dumpers.OSVersion;

    [SuppressUnmanagedCodeSecurity]
    internal static partial class Kernel32
    {
        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern uint GetCurrentProcessId();

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern uint GetCurrentThreadId();

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, EntryPoint = "GetVersionExW")]
        public static extern bool GetVersionEx([In, Out] OSVERSIONINFO osVersionInfo);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, EntryPoint = "GetVersionExW")]
        public static extern bool GetVersionEx([In, Out] OSVERSIONINFOEX osVersionInfoEx);

        [DllImport("ntdll.dll", SetLastError = false, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int RtlGetVersion([In, Out] OSVERSIONINFO osVersionInfo);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool GetProductInfo(int osMajor, int osMinor, int spMajor, int spMinor, out OSProductInfo productInfo);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern void GetNativeSystemInfo(out SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern int GetVersion();

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern bool IsWow64Process(IntPtr hProcess, out bool wow64);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern bool IsWow64Process2(IntPtr hProcess, out IMAGE_FILE_MACHINE processMachine, out IMAGE_FILE_MACHINE nativeMachine);
    }
}
