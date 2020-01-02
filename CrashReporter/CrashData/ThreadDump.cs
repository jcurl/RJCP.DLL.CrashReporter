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
    public class ThreadDump : ICrashDataExport
    {
        private const string ThreadTable = "OSThreads";
        private const string ThreadId = "id";
        private const string ThreadState = "state";
        private const string ThreadBasePrio = "basePrio";
        private const string ThreadPrio = "prio";
        private const string ThreadUserTime = "userTime";
        private const string ThreadTotalTime = "totalTime";

        private DumpRow m_Row = new DumpRow(
            ThreadId, ThreadState, ThreadBasePrio, ThreadPrio,
            ThreadUserTime, ThreadTotalTime);

        /// <summary>
        /// Dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public void Dump(ICrashDataDumpFile dumpFile)
        {
            using (IDumpTable table = dumpFile.DumpTable(ThreadTable, "thread")) {
                table.DumpHeader(m_Row);
                foreach (ProcessThread thread in Process.GetCurrentProcess().Threads) {
                    table.DumpRow(GetThreadInfo(thread, m_Row));
                }
                table.Flush();
            }
        }

        private DumpRow GetThreadInfo(ProcessThread thread, DumpRow row)
        {
            row[ThreadId] = thread.Id.ToString();
            row[ThreadState] = thread.ThreadState.ToString();
            row[ThreadBasePrio] = thread.BasePriority.ToString();
            row[ThreadPrio] = thread.PriorityLevel.ToString();
            row[ThreadUserTime] = thread.UserProcessorTime.TotalSeconds.ToString();
            row[ThreadTotalTime] = thread.TotalProcessorTime.TotalSeconds.ToString();
            return row;
        }

#if NET45
        /// <summary>
        /// Asynchronously dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public async Task DumpAsync(ICrashDataDumpFile dumpFile)
        {
            using (IDumpTable table = await dumpFile.DumpTableAsync(ThreadTable, "thread")) {
                await table.DumpHeaderAsync(m_Row);
                foreach (ProcessThread thread in Process.GetCurrentProcess().Threads) {
                    await table.DumpRowAsync(GetThreadInfo(thread, m_Row));
                }
                await table.FlushAsync();
            }
        }
#endif
    }
}
