namespace RJCP.Diagnostics.Native
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using Microsoft.Win32.SafeHandles;

    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        [DllImport("dbghelp.dll", SetLastError = true)]
        public static extern bool MiniDumpWriteDump(
            IntPtr hProcess,
            uint ProcessId,
            SafeFileHandle hFile,
            NativeMethods.MINIDUMP_TYPE DumpType,
            IntPtr ExceptionParam,
            IntPtr UserStreamParam,
            IntPtr CallackParam);
    }
}
