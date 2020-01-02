namespace RJCP.Diagnostics.CrashData
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using CrashExport;
#if NET45
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// Dump details of the running process.
    /// </summary>
    public class ProcessDump : ICrashDataExport
    {
        private const string ProcInfoTable = "ProcessInfo";
        private const string ProcInfoItem = "property";
        private const string ProcInfoValue = "value";

        private DumpRow m_Row = new DumpRow(ProcInfoItem, ProcInfoValue);

        /// <summary>
        /// Dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public void Dump(ICrashDataDumpFile dumpFile)
        {
            using (IDumpTable table = dumpFile.DumpTable(ProcInfoTable, "item")) {
                table.DumpHeader(m_Row);
                foreach (var item in GetOsInfo()) {
                    m_Row[ProcInfoItem] = item.Key;
                    m_Row[ProcInfoValue] = item.Value;
                    table.DumpRow(m_Row);
                }
                table.Flush();
            }
        }

        private IList<KeyValuePair<string, string>> GetOsInfo()
        {
            Process process = Process.GetCurrentProcess();
            return new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("name", process.ProcessName),
                new KeyValuePair<string, string>("id", process.Id.ToString()),
                new KeyValuePair<string, string>("basePrio", process.BasePriority.ToString()),
                new KeyValuePair<string, string>("prioClass", process.PriorityClass.ToString()),
                new KeyValuePair<string, string>("memPaged", process.PagedMemorySize64.ToString()),
                new KeyValuePair<string, string>("memWorking", process.WorkingSet64.ToString()),
                new KeyValuePair<string, string>("memVirtual", process.VirtualMemorySize64.ToString()),
                new KeyValuePair<string, string>("memPrivate", process.PrivateMemorySize64.ToString()),
                new KeyValuePair<string, string>("memNonpagedSystem", process.NonpagedSystemMemorySize64.ToString()),
                new KeyValuePair<string, string>("memPagedSystem", process.PagedSystemMemorySize64.ToString()),
                new KeyValuePair<string, string>("memPeakPaged", process.PeakPagedMemorySize64.ToString()),
                new KeyValuePair<string, string>("memPeakVirtual", process.PeakVirtualMemorySize64.ToString()),
                new KeyValuePair<string, string>("userTime", process.UserProcessorTime.TotalSeconds.ToString()),
                new KeyValuePair<string, string>("totalTime", process.TotalProcessorTime.TotalSeconds.ToString()),
                new KeyValuePair<string, string>("privilegedTime", process.PrivilegedProcessorTime.TotalSeconds.ToString())
            };
        }

#if NET45
        /// <summary>
        /// Asynchronously dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public async Task DumpAsync(ICrashDataDumpFile dumpFile)
        {
            using (IDumpTable table = await dumpFile.DumpTableAsync(ProcInfoTable, "item")) {
                await table.DumpHeaderAsync(m_Row);
                foreach (var item in GetOsInfo()) {
                    m_Row[ProcInfoItem] = item.Key;
                    m_Row[ProcInfoValue] = item.Value;
                    await table.DumpRowAsync(m_Row);
                }
                await table.FlushAsync();
            }
        }
#endif
    }
}
