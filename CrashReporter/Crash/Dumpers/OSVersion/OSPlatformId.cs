namespace RJCP.Diagnostics.Crash.Dumpers.OSVersion
{
    /// <summary>
    /// List of various platform IDs provided by Microsoft.
    /// </summary>
    /// <remarks>
    /// This field is derived from <c>OSVERSIONINFO.dwPlatformId</c>.
    /// At this time, Microsoft only document WinNT (2) in MSDN.
    /// </remarks>
    internal enum OSPlatformId
    {
        /// <summary>
        /// Unknown PlatformId.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Win32 Subsystem.
        /// </summary>
        Win32s = 0,

        /// <summary>
        /// Windows 9x.
        /// </summary>
        Win9x = 1,

        /// <summary>
        /// Windows NT.
        /// </summary>
        WinNT = 2,

        /// <summary>
        /// Windows Embedded.
        /// </summary>
        WinCE = 3,
    }
}
