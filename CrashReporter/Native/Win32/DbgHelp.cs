namespace RJCP.Diagnostics.Native.Win32
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using Microsoft.Win32.SafeHandles;

    [SuppressUnmanagedCodeSecurity]
    internal static partial class DbgHelp
    {
        [DllImport("dbghelp.dll", SetLastError = true)]
        public static extern bool MiniDumpWriteDump(
            IntPtr hProcess,
            uint ProcessId,
            SafeFileHandle hFile,
            MINIDUMP_TYPE DumpType,
            IntPtr ExceptionParam,
            IntPtr UserStreamParam,
            IntPtr CallackParam);
    }
}
