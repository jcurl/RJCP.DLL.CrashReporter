namespace RJCP.Diagnostics.Native.Win32
{
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    [SupportedOSPlatform("windows")]
    internal static partial class NtDll
    {
        [DllImport("ntdll.dll", SetLastError = false, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int RtlGetVersion([In, Out] Kernel32.OSVERSIONINFOEX osVersionInfoEx);
    }
}
