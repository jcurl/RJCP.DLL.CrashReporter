﻿namespace RJCP.Diagnostics.Crash.Dumpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.Versioning;
    using Export;

    /// <summary>
    /// Dumps all the OS threads to the log file.
    /// </summary>
    /// <remarks>
    /// This dumper is not very useful, as it's difficult to map the OS threads to the .NET threads. Use WinDbg on the
    /// core and get the relevant information that way.
    /// </remarks>
    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    internal sealed class ThreadDump : CrashDataExport<ProcessThread>
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
            using (Process proc = Process.GetCurrentProcess()) {
                foreach (ProcessThread thread in proc.Threads) {
                    if (thread is not null) yield return thread;
                }
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
        protected override bool UpdateRow(ProcessThread item, DumpRow row)
        {
            try {
                row[ThreadId] = item.Id.ToString();
                row[ThreadState] = item.ThreadState.ToString();
                row[ThreadBasePrio] = item.BasePriority.ToString();
                row[ThreadPrio] = item.PriorityLevel.ToString();
                row[ThreadUserTime] = item.UserProcessorTime.TotalSeconds.ToString();
                row[ThreadTotalTime] = item.TotalProcessorTime.TotalSeconds.ToString();
            } catch (AccessViolationException) { // Ignore: Access denied
            } catch (System.ComponentModel.Win32Exception) { // Ignore: Access denied
            } catch (InvalidOperationException) {
                return false;
            }
            return true;
        }
    }
}
