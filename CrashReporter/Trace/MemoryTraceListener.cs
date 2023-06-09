namespace RJCP.Diagnostics.Trace
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using Crash;

    /// <summary>
    /// A <see cref="TraceListener"/> that uses an <see cref="IMemoryLog"/> for tracing.
    /// </summary>
    public class MemoryTraceListener : TraceListener
    {
        private readonly MemoryLogDump m_MemoryLog;

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
        public MemoryTraceListener(IMemoryLog logCollection)
        {
            m_MemoryLog = new MemoryLogDump(logCollection);

            InternalClock.Instance.Initialize();
            CrashData.Instance.Providers.Add(m_MemoryLog);
        }

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
            m_MemoryLog.Add(entry);
        }

        private readonly LineSplitter m_Line = new LineSplitter();

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
                m_MemoryLog.Add(entry);
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
                m_MemoryLog.Add(entry);
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
            m_MemoryLog.Add(entry);
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
            m_MemoryLog.Add(entry);
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
            m_MemoryLog.Add(entry);
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
            m_MemoryLog.Add(entry);
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
            m_MemoryLog.Add(entry);
        }

        /// <summary>
        /// Flushes messages pending with <see cref="Write(string)"/> to the log.
        /// </summary>
        public override void Flush()
        {
            if (m_Line.IsCached) WriteLine(null);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="MemoryTraceListener"/> and optionally releases the
        /// managed resources.
        /// </summary>
        /// <param name="disposing">
        /// true to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                CrashData.Instance.Providers.Remove(m_MemoryLog);
            }
            base.Dispose(disposing);
        }
    }
}
