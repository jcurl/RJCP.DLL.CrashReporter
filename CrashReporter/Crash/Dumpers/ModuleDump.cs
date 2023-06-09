namespace RJCP.Diagnostics.Crash.Dumpers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Export;

    /// <summary>
    /// Dumps all the OS threads to the log file.
    /// </summary>
    /// <remarks>
    /// This dumper is not very useful, as it's difficult to map the OS threads to the .NET threads. Use WinDbg on the
    /// core and get the relevant information that way.
    /// </remarks>
    public class ModuleDump : CrashDataExport<ProcessModule>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleDump"/> class.
        /// </summary>
        public ModuleDump() : base(new DumpRow(ModName, ModFileVersion, ModFileName, ModProdVersion,
            ModProdName, ModOrigFileName, ModFileDesc, ModMemSize, ModBaseAddress))
        { }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        protected override string TableName { get { return ModuleTable; } }

        /// <summary>
        /// Gets the name of the row.
        /// </summary>
        /// <value>The name of the row.</value>
        protected override string RowName { get { return "module"; } }

        /// <summary>
        /// An enumerable to get the objects that should be dumped.
        /// </summary>
        /// <returns>An enumerable object.</returns>
        protected override IEnumerable<ProcessModule> GetRows()
        {
            foreach (ProcessModule module in Process.GetCurrentProcess().Modules) {
                yield return module;
            }
        }

        /// <summary>
        /// Updates the row given an item.
        /// </summary>
        /// <param name="item">The item returned from <see cref="GetRows()"/>.</param>
        /// <param name="row">The row that should be updated.</param>
        /// <returns>
        /// Returns <see langword="true"/> if the operation was successful and can be added to the dump file, else
        /// <see langword="false"/> that there was a problem and this row should be skipped.
        /// </returns>
        protected override bool UpdateRow(ProcessModule item, DumpRow row)
        {
            row[ModName] = GetField(() => item.ModuleName);
            row[ModFileVersion] = GetField(() => item.FileVersionInfo.FileVersion);
            row[ModFileName] = GetField(() => item.FileVersionInfo.FileName);
            row[ModProdVersion] = GetField(() => item.FileVersionInfo.ProductVersion);
            row[ModProdName] = GetField(() => item.FileVersionInfo.ProductName);
            row[ModOrigFileName] = GetField(() => item.FileVersionInfo.OriginalFilename);
            row[ModFileDesc] = GetField(() => item.FileVersionInfo.FileDescription);
            row[ModMemSize] = GetField(() => item.ModuleMemorySize.ToString());
            row[ModBaseAddress] = GetField(() => item.BaseAddress.ToString());
            return true;
        }
    }
}
