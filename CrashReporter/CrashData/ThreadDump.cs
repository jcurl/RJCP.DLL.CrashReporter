namespace RJCP.Diagnostics.CrashData
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using CrashExport;

    /// <summary>
    /// Dumps all the OS threads to the log file.
    /// </summary>
    /// <remarks>
    /// This dumper is not very useful, as it's difficult to map the OS threads to the .NET threads. Use WinDbg on the
    /// core and get the relevant information that way.
    /// </remarks>
    public class ThreadDump : CrashDataExport<ProcessThread>
    {
        private const string ThreadTable = "OSThreads";
        private const string ThreadId = "id";
        private const string ThreadState = "state";
        private const string ThreadBasePrio = "basePrio";
        private const string ThreadPrio = "prio";
        private const string ThreadUserTime = "userTime";
        private const string ThreadTotalTime = "totalTime";

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadDump"/> class.
        /// </summary>
        public ThreadDump() : base(
            new DumpRow(ThreadId, ThreadState, ThreadBasePrio, ThreadPrio, ThreadUserTime, ThreadTotalTime))
        { }


        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        protected override string TableName { get { return ThreadTable; } }

        /// <summary>
        /// Gets the name of the row.
        /// </summary>
        /// <value>The name of the row.</value>
        protected override string RowName { get { return "thread"; } }

        /// <summary>
        /// An enumerable to get the objects that should be dumped.
        /// </summary>
        /// <returns>An enumerable object.</returns>
        protected override IEnumerable<ProcessThread> GetRows()
        {
            foreach (ProcessThread thread in Process.GetCurrentProcess().Threads) {
                if (thread != null) yield return thread;
            }
        }

        /// <summary>
        /// Updates the row given an item.
        /// </summary>
        /// <param name="item">The item returned from <see cref="GetRows()"/>.</param>
        /// <param name="row">The row that should be updated.</param>
        protected override void UpdateRow(ProcessThread item, DumpRow row)
        {
            row[ThreadId] = item.Id.ToString();
            row[ThreadState] = item.ThreadState.ToString();
            row[ThreadBasePrio] = item.BasePriority.ToString();
            row[ThreadPrio] = item.PriorityLevel.ToString();
            row[ThreadUserTime] = item.UserProcessorTime.TotalSeconds.ToString();
            row[ThreadTotalTime] = item.TotalProcessorTime.TotalSeconds.ToString();
        }
    }
}
