namespace RJCP.Diagnostics.Logging
{
    using System;
    using Microsoft.Extensions.Logging;
    using Trace;

    internal sealed class MemoryLogger : ILogger
    {
        private readonly string m_Category;
        private readonly LogLevel m_MinLogLevel;
        private readonly MemoryLogDump m_MemoryLog;

        internal MemoryLogger(string category, LogLevel minLogLevel, MemoryLogDump memoryLog)
        {
            m_Category = category;
            m_MinLogLevel = minLogLevel;
            m_MemoryLog = memoryLog;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return default;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= m_MinLogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            string message = formatter(state, exception);
            LogEntry entry = new(logLevel, eventId.Id, message) {
                DateTime = DateTime.Now,
                Source = m_Category
            };
            m_MemoryLog.Add(entry);
        }
    }
}
