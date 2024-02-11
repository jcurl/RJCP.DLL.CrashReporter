namespace RJCP.Diagnostics.Crash.Dumpers.OSVersion
{
    using System.Runtime.Versioning;
    using Native.Win32;

    [SupportedOSPlatform("windows")]
    internal static class OSArchitecture
    {
        public static string GetImageFileMachineString(Kernel32.IMAGE_FILE_MACHINE imageFileMachine)
        {
            switch (imageFileMachine) {
            case Kernel32.IMAGE_FILE_MACHINE.UNKNOWN: return "Unknown";
            case Kernel32.IMAGE_FILE_MACHINE.TARGET_HOST: return "Target HOST";
            case Kernel32.IMAGE_FILE_MACHINE.I386: return "Intel x86 32-bit";
            case Kernel32.IMAGE_FILE_MACHINE.MIPS_R3000: return "MIPS Little-Endian R3000";
            case Kernel32.IMAGE_FILE_MACHINE.MIPS_R3000_BE: return "MIPS Big-Endian R3000";
            case Kernel32.IMAGE_FILE_MACHINE.MIPS_R4000: return "MIPS Little-Endian R4000";
            case Kernel32.IMAGE_FILE_MACHINE.MIPS_R10000: return "MIPS Little-Endian R10000";
            case Kernel32.IMAGE_FILE_MACHINE.MIPS_WCEMIPSV2: return "MIPS Little-Endian WCE v2";
            case Kernel32.IMAGE_FILE_MACHINE.ALPHA: return "AXP Alpha";
            case Kernel32.IMAGE_FILE_MACHINE.ALPHA64: return "AXP Alpha 64";
            case Kernel32.IMAGE_FILE_MACHINE.SH3: return "SH3 Little-Endian";
            case Kernel32.IMAGE_FILE_MACHINE.SH3DSP: return "SH3 DSP";
            case Kernel32.IMAGE_FILE_MACHINE.SH3E: return "SH3E Little-Endian";
            case Kernel32.IMAGE_FILE_MACHINE.SH4: return "SH4 Little-Endian";
            case Kernel32.IMAGE_FILE_MACHINE.SH5: return "SH5";
            case Kernel32.IMAGE_FILE_MACHINE.ARM: return "ARM Little-Endian";
            case Kernel32.IMAGE_FILE_MACHINE.ARM_THUMB: return "ARM Thumb/Thumb2 Little-Endian";
            case Kernel32.IMAGE_FILE_MACHINE.ARMNT: return "ARM Thumb2 Little-Endian";
            case Kernel32.IMAGE_FILE_MACHINE.ARM64: return "ARM 64-bit";
            case Kernel32.IMAGE_FILE_MACHINE.AM33: return "AM33";
            case Kernel32.IMAGE_FILE_MACHINE.POWERPC: return "Power PC";
            case Kernel32.IMAGE_FILE_MACHINE.POWERPCFP: return "Power PC FP";
            case Kernel32.IMAGE_FILE_MACHINE.IA64: return "Intel 64";
            case Kernel32.IMAGE_FILE_MACHINE.MIPS16: return "MIPS 16";
            case Kernel32.IMAGE_FILE_MACHINE.MIPSFPU: return "MIPS FPU";
            case Kernel32.IMAGE_FILE_MACHINE.MIPSFPU16: return "MIPS FPU16";
            case Kernel32.IMAGE_FILE_MACHINE.TRICORE: return "Infineon TriCore";
            case Kernel32.IMAGE_FILE_MACHINE.AMD64: return "AMD 64-bit";
            case Kernel32.IMAGE_FILE_MACHINE.M32R: return "M32R Little-Endian";
            case Kernel32.IMAGE_FILE_MACHINE.CEF: return "CEF";
            case Kernel32.IMAGE_FILE_MACHINE.CEE: return "CEE";
            case Kernel32.IMAGE_FILE_MACHINE.EBC: return "EBC";
            default: return $"Unknown Image File Machine {imageFileMachine}";
            }
        }

        public static string GetProcessArchitecture(ushort processorArchitecture)
        {
            switch (processorArchitecture) {
            case Kernel32.PROCESSOR_ARCHITECTURE.INTEL: return "Intel x86 32-bit";
            case Kernel32.PROCESSOR_ARCHITECTURE.MIPS: return "MIPS";
            case Kernel32.PROCESSOR_ARCHITECTURE.ALPHA: return "AXP Alpha";
            case Kernel32.PROCESSOR_ARCHITECTURE.PPC: return "Power PC";
            case Kernel32.PROCESSOR_ARCHITECTURE.SHX: return "SHx";
            case Kernel32.PROCESSOR_ARCHITECTURE.ARM: return "ARM Little-Endian";
            case Kernel32.PROCESSOR_ARCHITECTURE.IA64: return "Intel 64";
            case Kernel32.PROCESSOR_ARCHITECTURE.MSIL: return "Microsoft Intermediate Language";
            case Kernel32.PROCESSOR_ARCHITECTURE.AMD64: return "AMD 64-bit";
            case Kernel32.PROCESSOR_ARCHITECTURE.ARM64: return "ARM 64-bit";
            default: return $"Unknown Processor Architecture {processorArchitecture}";
            }
        }
    }
}
