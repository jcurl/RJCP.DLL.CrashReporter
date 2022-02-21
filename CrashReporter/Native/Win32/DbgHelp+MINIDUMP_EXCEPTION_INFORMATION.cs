namespace RJCP.Diagnostics.Native.Win32
{
    using System;
    using System.Runtime.InteropServices;

    internal static partial class DbgHelp
    {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct MINIDUMP_EXCEPTION_INFORMATION
        {
            public uint ThreadId;
            public IntPtr ExceptionPointers;
            public int ClientPointers;
        }
    }
}
