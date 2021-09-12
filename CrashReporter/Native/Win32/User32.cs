namespace RJCP.Diagnostics.Native.Win32
{
    using System.Runtime.InteropServices;

    internal static partial class User32
    {
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int GetSystemMetrics(uint nIndex);
    }
}
