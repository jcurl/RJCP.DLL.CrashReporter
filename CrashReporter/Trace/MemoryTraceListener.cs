namespace RJCP.Diagnostics.Trace
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using CrashExport;
    using Dump;
#if NET45_OR_GREATER
    using System.Collections.Generic;
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// A <see cref="TraceListener"/> that uses an <see cref="IMemoryLog"/> for tracing.
    /// </summary>
    public abstract class MemoryTraceListener : TraceListener, ICrashDataExport
    {
        private readonly object m_SyncLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryTraceListener"/> class.
        /// </summary>
        /// <param name="logCollection">The log collection.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="logCollection"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// This object implements thread safe locking around access to <paramref name="logCollection"/>. To maintain
        /// thread safety, ensure that direct access to this collection do not happen in parallel with this class.
        /// </remarks>
        protected MemoryTraceListener(IMemoryLog logCollection)
        {
            if (logCollection == null) throw new ArgumentNullException(nameof(logCollection));
            InternalClock.Instance.Initialize();
            Crash.Data.Providers.Add(this);
            MemoryLog = logCollection;
        }

        /// <summary>
        /// Gets the m memory log collection that was given to this constructor.
        /// </summary>
        /// <value>The m memory log collection that was given to this constructor.</value>
        /// <remarks>
        /// This object implements thread safe locking around access to <see cref="MemoryLog"/>. To maintain thread
        /// safety, ensure that direct access to this collection do not happen in parallel with this class.
        /// </remarks>
        protected IMemoryLog MemoryLog { get; private set; }

        /// <summary>
        /// Indicates if this is a default logging source.
        /// </summary>
        /// <value>
        /// If <see langword="true"/> the first registered logger will be used for crash logging by default. Otherwise
        /// if <see langword="false"/>, it may be used only if there are no defaults.
        /// </value>
        public bool DefaultSource { get; protected set; }

        private bool m_Init;

        private void CheckInit()
        {
            if (!m_Init) {
                TraceInit();
                m_Init = true;
            }
        }

        /// <summary>
        /// Initializes the trace data on the first call to log something.
        /// </summary>
        protected virtual void TraceInit() { }

        /// <summary>
        /// Emits an error message and a detailed error message to the listener you create when you implement the
        /// <see cref="TraceListener"/> class.
        /// </summary>
        /// <param name="message">A message to emit.</param>
        /// <param name="detailMessage">A detailed message to emit.</param>
        /// <remarks>Messages marked as failure are logged with <see cref="TraceEventType.Warning"/>.</remarks>
        public override void Fail(string message, string detailMessage)
        {
            CheckInit();
            if (m_Line.IsCached) WriteLine(null);

            string logMessage;
            if (detailMessage == null) {
                logMessage = message;
            } else {
                logMessage = new StringBuilder().Append(message).Append(": ").Append(detailMessage).ToString();
            }
            LogEntry entry = new LogEntry(TraceEventType.Warning, 0, logMessage) {
                DateTime = DateTime.Now
            };
            lock (m_SyncLock) { MemoryLog.Add(entry); }
        }

        private LineSplitter m_Line = new LineSplitter();

        /// <summary>
        /// Writes the specified message to the listener.
        /// </summary>
        /// <param name="message">A message to write.</param>
        /// <remarks>Messages are logged with <see cref="TraceEventType.Verbose"/>.</remarks>
        public override void Write(string message)
        {
            CheckInit();

            foreach (string line in m_Line.Append(message)) {
                LogEntry entry = new LogEntry(TraceEventType.Verbose, 0, line) {
                    DateTime = DateTime.Now
                };
                lock (m_SyncLock) { MemoryLog.Add(entry); }
            }
        }

        /// <summary>
        /// Writes the specified message to the listener.
        /// </summary>
        /// <param name="message">A message to write.</param>
        /// <remarks>Messages are logged with <see cref="TraceEventType.Verbose"/>.</remarks>
        public override void WriteLine(string message)
        {
            CheckInit();

            foreach (string line in m_Line.AppendLine(message)) {
                LogEntry entry = new LogEntry(TraceEventType.Verbose, 0, line) {
                    DateTime = DateTime.Now
                };
                lock (m_SyncLock) { MemoryLog.Add(entry); }
            }
        }

        /// <summary>
        /// Writes trace and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">
        /// A <see cref="TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace
        /// information.
        /// </param>
        /// <param name="source">
        /// A name used to identify the output, typically the name of the application that generated the trace event.
        /// </param>
        /// <param name="eventType">
        /// One of the <see cref="TraceEventType"/> values specifying the type of event that has caused the trace.
        /// </param>
        /// <param name="id">A numeric identifier for the event.</param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            CheckInit();
            if (m_Line.IsCached) WriteLine(null);

            LogEntry entry = new LogEntry(eventType, id, string.Empty) {
                DateTime = eventCache.DateTime,
                Source = source,
                ThreadId = eventCache.ThreadId
            };
            lock (m_SyncLock) { MemoryLog.Add(entry); }
        }

        /// <summary>
        /// Writes trace information, a message, and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">
        /// A <see cref="TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace
        /// information.
        /// </param>
        /// <param name="source">
        /// A name used to identify the output, typically the name of the application that generated the trace event.
        /// </param>
        /// <param name="eventType">
        /// One of the <see cref="TraceEventType"/> values specifying the type of event that has caused the trace.
        /// </param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">A message to write.</param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            CheckInit();
            if (m_Line.IsCached) WriteLine(null);

            LogEntry entry = new LogEntry(eventType, id, message) {
                DateTime = eventCache.DateTime,
                Source = source,
                ThreadId = eventCache.ThreadId
            };
            lock (m_SyncLock) { MemoryLog.Add(entry); }
        }

        /// <summary>
        /// Writes trace information, a formatted array of objects and event information to the listener specific
        /// output.
        /// </summary>
        /// <param name="eventCache">
        /// A <see cref="TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace
        /// information.
        /// </param>
        /// <param name="source">
        /// A name used to identify the output, typically the name of the application that generated the trace event.
        /// </param>
        /// <param name="eventType">
        /// One of the <see cref="TraceEventType"/> values specifying the type of event that has caused the trace.
        /// </param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="format">
        /// A format string that contains zero or more format items, which correspond to objects in the
        /// <paramref name="args"/> array.
        /// </param>
        /// <param name="args">An <see langword="object"/> array containing zero or more objects to format.</param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            CheckInit();
            if (m_Line.IsCached) WriteLine(null);

            string message = string.Format(format, args);
            LogEntry entry = new LogEntry(eventType, id, message) {
                DateTime = eventCache.DateTime,
                Source = source,
                ThreadId = eventCache.ThreadId
            };
            lock (m_SyncLock) { MemoryLog.Add(entry); }
        }

        /// <summary>
        /// Writes trace information, a data object and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">
        /// A <see cref="TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace
        /// information.
        /// </param>
        /// <param name="source">
        /// A name used to identify the output, typically the name of the application that generated the trace event.
        /// </param>
        /// <param name="eventType">
        /// One of the <see cref="TraceEventType"/> values specifying the type of event that has caused the trace.
        /// </param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="data">The trace data to emit.</param>
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            CheckInit();
            if (m_Line.IsCached) WriteLine(null);

            string message = data.ToString();
            LogEntry entry = new LogEntry(eventType, id, message) {
                DateTime = eventCache.DateTime,
                Source = source,
                ThreadId = eventCache.ThreadId
            };
            lock (m_SyncLock) { MemoryLog.Add(entry); }
        }

        /// <summary>
        /// Writes trace information, an array of data objects and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">
        /// A <see cref="TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace
        /// information.
        /// </param>
        /// <param name="source">
        /// A name used to identify the output, typically the name of the application that generated the trace event.
        /// </param>
        /// <param name="eventType">
        /// One of the <see cref="TraceEventType"/> values specifying the type of event that has caused the trace.
        /// </param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="data">An array of objects to emit as data.</param>
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            CheckInit();
            if (m_Line.IsCached) WriteLine(null);

            string message;
            if (data != null) {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < data.Length; i++) {
                    if (i != 0)
                        sb.Append(", ");
                    if (data[i] != null)
                        sb.Append(data[i].ToString());
                }
                message = sb.ToString();
            } else {
                message = string.Empty;
            }

            LogEntry entry = new LogEntry(eventType, id, message) {
                DateTime = eventCache.DateTime,
                Source = source,
                ThreadId = eventCache.ThreadId
            };
            lock (m_SyncLock) { MemoryLog.Add(entry); }
        }

        /// <summary>
        /// Flushes messages pending with <see cref="Write(string)"/> to the log.
        /// </summary>
        public override void Flush()
        {
            if (m_Line.IsCached) WriteLine(null);
        }

        private const string LogTable = "TraceListenerLog";
        private const string LogInternalClock = "clock";
        private const string LogDateTime = "timestamp";
        private const string LogEventType = "eventType";
        private const string LogSource = "source";
        private const string LogId = "id";
        private const string LogThreadId = "threadid";
        private const string LogMessage = "message";

        private DumpRow m_Row = new DumpRow(
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
                    foreach (var entry in MemoryLog) {
                        table.DumpRow(GetLogEntry(entry, m_Row));
                    }
                }
                table.Flush();
            }
        }

        private DumpRow GetLogEntry(LogEntry entry, DumpRow row)
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

#if NET45_OR_GREATER
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
                    foreach (var entry in MemoryLog) {
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
