namespace RJCP.Diagnostics.Native.Win32
{
    internal static partial class Kernel32
    {
        public enum IMAGE_FILE_MACHINE : ushort
        {
            UNKNOWN = 0,

            /// <summary>
            /// Useful for indicating we want to interact with the host and not a WoW guest.
            /// </summary>
            TARGET_HOST = 0x0001,

            /// <summary>
            /// Intel 386.
            /// </summary>
            I386 = 0x014c,

            /// <summary>
            /// MIPS big-endian
            /// </summary>
            MIPS_R3000_BE = 0x0160,

            /// <summary>
            /// MIPS little-endian
            /// </summary>
            MIPS_R3000 = 0x0162,

            /// <summary>
            /// MIPS little-endian.
            /// </summary>
            MIPS_R4000 = 0x0166,

            /// <summary>
            /// MIPS little-endian
            /// </summary>
            MIPS_R10000 = 0x0168,

            /// <summary>
            /// MIPS little-endian WCE v2.
            /// </summary>
            MIPS_WCEMIPSV2 = 0x0169,

            /// <summary>
            /// Alpha_AXP.
            /// </summary>
            ALPHA = 0x0184,

            /// <summary>
            /// SH3 little-endian.
            /// </summary>
            SH3 = 0x01a2,

            SH3DSP = 0x01a3,

            /// <summary>
            /// SH3E little-endian.
            /// </summary>
            SH3E = 0x01a4,

            /// <summary>
            /// SH4 little-endian.
            /// </summary>
            SH4 = 0x01a6,

            /// <summary>
            /// SH5.
            /// </summary>
            SH5 = 0x01a8,

            /// <summary>
            /// ARM Little-Endian.
            /// </summary>
            ARM = 0x01c0,

            /// <summary>
            /// ARM Thumb/Thumb-2 Little-Endian.
            /// </summary>
            ARM_THUMB = 0x01c2,

            /// <summary>
            /// ARM Thumb-2 Little-Endian.
            /// </summary>
            ARMNT = 0x01c4,

            AM33 = 0x01d3,

            /// <summary>
            /// IBM PowerPC Little-Endian.
            /// </summary>
            POWERPC = 0x01F0,

            POWERPCFP = 0x01f1,

            /// <summary>
            /// Intel 64.
            /// </summary>
            IA64 = 0x0200,

            /// <summary>
            /// MIPS.
            /// </summary>
            MIPS16 = 0x0266,

            /// <summary>
            /// ALPHA64.
            /// </summary>
            ALPHA64 = 0x0284,

            /// <summary>
            /// MIPS.
            /// </summary>
            MIPSFPU = 0x0366,

            /// <summary>
            /// MIPS.
            /// </summary>
            MIPSFPU16 = 0x0466,

            AXP64 = ALPHA64,

            /// <summary>
            /// Infineon.
            /// </summary>
            TRICORE = 0x0520,

            CEF = 0x0CEF,

            /// <summary>
            /// EFI Byte Code.
            /// </summary>
            EBC = 0x0EBC,

            /// <summary>
            /// AMD64 (K8),
            /// </summary>
            AMD64 = 0x8664,

            /// <summary>
            /// M32R little-endian.
            /// </summary>
            M32R = 0x9041,

            /// <summary>
            /// ARM64 Little-Endian.
            /// </summary>
            ARM64 = 0xAA64,

            CEE = 0xC0EE
        }
    }
}
