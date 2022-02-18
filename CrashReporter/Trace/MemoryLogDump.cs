﻿namespace RJCP.Diagnostics.Trace
{
    using System;
    using CrashExport;
#if NET45_OR_GREATER || NETSTANDARD
    using System.Collections.Generic;
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// Provides a common implementation for collecting logs and being able to dump them to a crash report.
    /// </summary>
    internal class MemoryLogDump : ICrashDataExport
    {
        private readonly IMemoryLog m_MemoryLog;
        private readonly object m_SyncLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryLogDump"/> class.
        /// </summary>
        /// <param name="logCollection">The log collection.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="logCollection"/> is <see langword="null"/>.
        /// </exception>
        public MemoryLogDump(IMemoryLog logCollection)
        {
            if (logCollection == null) throw new ArgumentNullException(nameof(logCollection));
            m_MemoryLog = logCollection;
        }

        /// <summary>
        /// Adds the specified item to the log.
        /// </summary>
        /// <param name="item">The log item.</param>
        public void Add(LogEntry item)
        {
            lock (m_SyncLock) {
                m_MemoryLog.Add(item);
            }
        }

        private const string LogTable = "TraceListenerLog";
        private const string LogInternalClock = "clock";
        private const string LogDateTime = "timestamp";
        private const string LogEventType = "eventType";
        private const string LogSource = "source";
        private const string LogId = "id";
        private const string LogThreadId = "threadid";
        private const string LogMessage = "message";

        private readonly DumpRow m_Row = new DumpRow(
            LogInternalClock, LogDateTime, LogEventType, LogSource,
            LogId, LogThreadId, LogMessage);

        /// <summary>
        /// Dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public void Dump(ICrashDataDumpFile dumpFile)
        {
            using (IDumpTable table = dumpFile.DumpTable(LogTable, "entry")) {
                table.DumpHeader(m_Row);
                lock (m_SyncLock) {
                    foreach (var entry in m_MemoryLog) {
                        table.DumpRow(GetLogEntry(entry, m_Row));
                    }
                }
                table.Flush();
            }
        }

        private static DumpRow GetLogEntry(LogEntry entry, DumpRow row)
        {
            row[LogInternalClock] = entry.Clock.ToString();
            row[LogDateTime] = (entry.DateTime.Ticks == 0) ? string.Empty : entry.DateTime.ToString("o");
            row[LogEventType] = entry.EventType.ToString();
            row[LogSource] = entry.Source;
            row[LogId] = entry.Id.ToString();
            row[LogThreadId] = entry.ThreadId;
            row[LogMessage] = entry.Message;
            return row;
        }

#if NET45_OR_GREATER || NETSTANDARD
        /// <summary>
        /// Asynchronously dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        /// <returns>An awaitable task.</returns>
        public async Task DumpAsync(ICrashDataDumpFile dumpFile)
        {
            using (IDumpTable table = await dumpFile.DumpTableAsync(LogTable, "entry")) {
                await table.DumpHeaderAsync(m_Row);

                // We can't use awaitable inside a lock, so we need to first copy the data.
                List<LogEntry> list = new List<LogEntry>();
                lock (m_SyncLock) {
                    foreach (var entry in m_MemoryLog) {
                        list.Add(entry);
                    }
                }

                foreach (var entry in list) {
                    await table.DumpRowAsync(GetLogEntry(entry, m_Row));
                }
                await table.FlushAsync();
            }
        }
#endif
    }
}
