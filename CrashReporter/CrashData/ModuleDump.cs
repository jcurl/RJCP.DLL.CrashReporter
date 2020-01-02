namespace RJCP.Diagnostics.CrashData
{
    using System.Diagnostics;
    using CrashExport;
#if NET45
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// Dumps all the OS threads to the log file.
    /// </summary>
    /// <remarks>
    /// This dumper is not very useful, as it's difficult to map the OS threads to the .NET threads. Use WinDbg on the
    /// core and get the relevant information that way.
    /// </remarks>
    public class ModuleDump : ICrashDataExport
    {
        private const string ModuleTable = "Modules";
        private const string ModName = "name";
        private const string ModFileVersion = "fileVersion";
        private const string ModFileName = "fileName";
        private const string ModProdVersion = "productVersion";
        private const string ModProdName = "productName";
        private const string ModOrigFileName = "originalFileName";
        private const string ModFileDesc = "fileDesc";
        private const string ModMemSize = "memorySize";
        private const string ModBaseAddress = "baseAddress";

        private DumpRow m_Row = new DumpRow(
            ModName, ModFileVersion, ModFileName, ModProdVersion,
            ModProdName, ModOrigFileName, ModFileDesc, ModMemSize, ModBaseAddress);

        /// <summary>
        /// Dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public void Dump(ICrashDataDumpFile dumpFile)
        {
            using (IDumpTable table = dumpFile.DumpTable(ModuleTable, "module")) {
                table.DumpHeader(m_Row);
                foreach (ProcessModule module in Process.GetCurrentProcess().Modules) {
                    table.DumpRow(GetModuleInfo(module, m_Row));
                }
                table.Flush();
            }
        }

        private DumpRow GetModuleInfo(ProcessModule module, DumpRow row)
        {
            row[ModName] = module.ModuleName;
            row[ModFileVersion] = module.FileVersionInfo.FileVersion;
            row[ModFileName] = module.FileVersionInfo.FileName;
            row[ModProdVersion] = module.FileVersionInfo.ProductVersion;
            row[ModProdName] = module.FileVersionInfo.ProductName;
            row[ModOrigFileName] = module.FileVersionInfo.OriginalFilename;
            row[ModFileDesc] = module.FileVersionInfo.FileDescription;
            row[ModMemSize] = module.ModuleMemorySize.ToString();
            row[ModBaseAddress] = module.BaseAddress.ToString();
            return row;
        }

#if NET45
        /// <summary>
        /// Asynchronously dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public async Task DumpAsync(ICrashDataDumpFile dumpFile)
        {
            using (IDumpTable table = await dumpFile.DumpTableAsync(ModuleTable, "module")) {
                await table.DumpHeaderAsync(m_Row);
                foreach (ProcessModule module in Process.GetCurrentProcess().Modules) {
                    await table.DumpRowAsync(GetModuleInfo(module, m_Row));
                }
                await table.FlushAsync();
            }
        }
#endif
    }
}
