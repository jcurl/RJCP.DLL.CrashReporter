namespace RJCP.Diagnostics.Trace
{
    using System;
    using System.Diagnostics;
#if NETSTANDARD
    using Microsoft.Extensions.Logging;
#endif

    /// <summary>
    /// A Trace Log Entry.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogEntry"/> class with the default clock.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="id">The event identifier.</param>
        /// <param name="message">The event message.</param>
        public LogEntry(TraceEventType eventType, int id, string message)
        {
            Clock = InternalClock.Instance.GetClock();
            EventType = eventType;
            Id = id;
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEntry"/> class with the given clock.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="id">The event identifier.</param>
        /// <param name="message">The event message.</param>
        /// <param name="clock">The clock when the event occurred in milliseconds.</param>
        public LogEntry(TraceEventType eventType, int id, string message, long clock)
        {
            Clock = clock;
            EventType = eventType;
            Id = id;
            Message = message;
        }

#if NETSTANDARD
        /// <summary>
        /// Initializes a new instance of the <see cref="LogEntry"/> class with the default clock.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="id">The event identifier.</param>
        /// <param name="message">The event message.</param>
        [CLSCompliant(false)]
        public LogEntry(LogLevel level, int id, string message)
        {
            Clock = InternalClock.Instance.GetClock();
            EventType = GetTraceEventType(level);
            Id = id;
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEntry"/> class with the given clock.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="id">The event identifier.</param>
        /// <param name="message">The event message.</param>
        /// <param name="clock">The clock when the event occurred in milliseconds.</param>
        [CLSCompliant(false)]
        public LogEntry(LogLevel level, int id, string message, long clock)
        {
            Clock = clock;
            EventType = GetTraceEventType(level);
            Id = id;
            Message = message;
        }

        private static TraceEventType GetTraceEventType(LogLevel level)
        {
            switch (level) {
            case LogLevel.None:
            case LogLevel.Critical:
                return TraceEventType.Critical;
            case LogLevel.Error:
                return TraceEventType.Error;
            case LogLevel.Warning:
                return TraceEventType.Warning;
            case LogLevel.Information:
                return TraceEventType.Information;
            case LogLevel.Debug:
            case LogLevel.Trace:
                return TraceEventType.Verbose;
            default:
                return TraceEventType.Verbose;
            }
        }
#endif

        /// <summary>
        /// A 64-bit clock time stamp, which starts at zero near the time when the program starts.
        /// </summary>
        /// <value>
        /// The 64-bit clock in milliseconds.
        /// </value>
        public long Clock { get; private set; }

        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        /// <value>
        /// The type of the event.
        /// </value>
        public TraceEventType EventType { get; private set; }

        /// <summary>
        /// Gets the source of the event.
        /// </summary>
        /// <value>
        /// The source of the event.
        /// </value>
        public string Source { get; set; }

        /// <summary>
        /// Gets the identifier of the event.
        /// </summary>
        /// <value>
        /// The identifier of the event.
        /// </value>
        public int Id { get; private set; }

        /// <summary>
        /// Gets or sets the date and time of the event.
        /// </summary>
        /// <value>
        /// The date and time of the event.
        /// </value>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Gets the thread identifier on which the event occurred.
        /// </summary>
        /// <value>
        /// The thread identifier on which the event occurred.
        /// </value>
        public string ThreadId { get; set; }

        /// <summary>
        /// Gets the event message.
        /// </summary>
        /// <value>
        /// The event message.
        /// </value>
        public string Message { get; private set; }
    }
}
