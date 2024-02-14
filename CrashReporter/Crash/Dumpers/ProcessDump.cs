namespace RJCP.Diagnostics.Crash.Dumpers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Export;

    /// <summary>
    /// Dump details of the running process.
    /// </summary>
    internal sealed class ProcessDump : CrashDataExport<KeyValuePair<string, string>>
    {
        private const string ProcInfoTable = "ProcessInfo";
        private const string ProcInfoItem = "property";
        private const string ProcInfoValue = "value";

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessDump" /> class.
        /// </summary>
        public ProcessDump() : base(new DumpRow(ProcInfoItem, ProcInfoValue)) { }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        protected override string TableName { get { return ProcInfoTable; } }

        /// <summary>
        /// Gets the name of the row.
        /// </summary>
        /// <value>The name of the row.</value>
        protected override string RowName { get { return "item"; } }

        /// <summary>
        /// An enumerable to get the objects that should be dumped.
        /// </summary>
        /// <returns>An enumerable object.</returns>
        protected override IEnumerable<KeyValuePair<string, string>> GetRows()
        {
            using (Process process = Process.GetCurrentProcess()) {
                return new List<KeyValuePair<string, string>>() {
                    new("name", ProcessInfo.ProcessName),
                    new("id", ProcessInfo.ProcessId.ToString()),
                    new("basePrio", process.BasePriority.ToString()),
                    new("prioClass", process.PriorityClass.ToString()),
                    new("memPaged", process.PagedMemorySize64.ToString()),
                    new("memWorking", process.WorkingSet64.ToString()),
                    new("memVirtual", process.VirtualMemorySize64.ToString()),
                    new("memPrivate", process.PrivateMemorySize64.ToString()),
                    new("memNonpagedSystem", process.NonpagedSystemMemorySize64.ToString()),
                    new("memPagedSystem", process.PagedSystemMemorySize64.ToString()),
                    new("memPeakPaged", process.PeakPagedMemorySize64.ToString()),
                    new("memPeakVirtual", process.PeakVirtualMemorySize64.ToString()),
                    new("userTime", process.UserProcessorTime.TotalSeconds.ToString()),
                    new("totalTime", process.TotalProcessorTime.TotalSeconds.ToString()),
                    new("privilegedTime", process.PrivilegedProcessorTime.TotalSeconds.ToString())
                };
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
        protected override bool UpdateRow(KeyValuePair<string, string> item, DumpRow row)
        {
            row[ProcInfoItem] = item.Key;
            row[ProcInfoValue] = item.Value;
            return true;
        }
    }
}
