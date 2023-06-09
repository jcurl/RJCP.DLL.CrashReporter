namespace RJCP.Diagnostics.Logging
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using Crash;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Trace;

    /// <summary>
    /// A provider for a <see cref="MemoryLogger"/> with simple prioritised queues.
    /// </summary>
    [ProviderAlias("SimplePrioMemoryLogger")]
    [CLSCompliant(false)]
    public sealed class SimplePrioMemoryLogProvider : ILoggerProvider
    {
        private readonly LogLevel m_MinLevel;
        private readonly IDisposable m_OnChangeToken;
        private SimplePrioMemoryLogConfig m_CurrentConfig;
        private readonly ConcurrentDictionary<string, MemoryLogger> m_Loggers =
            new ConcurrentDictionary<string, MemoryLogger>(StringComparer.OrdinalIgnoreCase);

        private readonly object m_Lock = new object();
        private MemoryLogDump m_MemoryLog;
        private IMemoryLog m_LogCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimplePrioMemoryLogProvider"/> class with logging set to a
        /// minimum level.
        /// </summary>
        /// <param name="minLevel">The minimum level for logging.</param>
        public SimplePrioMemoryLogProvider(LogLevel minLevel)
        {
            m_MinLevel = minLevel;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimplePrioMemoryLogProvider"/> class.
        /// </summary>
        /// <param name="config">The configuration monitor.</param>
        /// <param>
        /// This constructor is usually called by the Host. Instantiate the provider with the
        /// <see cref="MemoryLoggerExtensions.AddSimplePrioMemoryLogger(ILoggingBuilder)"/>.
        /// </param>
        public SimplePrioMemoryLogProvider(IOptionsMonitor<SimplePrioMemoryLogConfig> config)
            : this(LogLevel.Trace)
        {
            // Filter would be applied at the LoggerFactory level
            m_CurrentConfig = config.CurrentValue;
            m_OnChangeToken = config.OnChange(updatedConfig => {
                m_CurrentConfig = updatedConfig;
            });
        }

        /// <summary>
        /// Creates the logger with the current configuration.
        /// </summary>
        /// <param name="categoryName">Name of the category.</param>
        /// <returns>A logger</returns>
        /// <remarks>
        /// The memory log configuration for the logger is defined at the time the first logger is created. All
        /// subsequent loggers created use the first memory logger that was initialized.
        /// <para>
        /// Creating the logger and the first memory logger automatically registers it with the crash provider.
        /// </para>
        /// <para>This results in multiple loggers created with this provider use the same memory logger.</para>
        /// </remarks>
        public ILogger CreateLogger(string categoryName)
        {
            if (m_MemoryLog == null) {
                lock (m_Lock) {
                    if (m_MemoryLog == null) {
                        InternalClock.Instance.Initialize();

                        SimplePrioMemoryLog log = new SimplePrioMemoryLog();
                        if (m_CurrentConfig != null) {
                            try {
                                if (m_CurrentConfig.Critical != 0) log.Critical = m_CurrentConfig.Critical;
                                if (m_CurrentConfig.Error != 0) log.Error = m_CurrentConfig.Error;
                                if (m_CurrentConfig.Warning != 0) log.Warning = m_CurrentConfig.Warning;
                                if (m_CurrentConfig.Info != 0) log.Info = m_CurrentConfig.Info;
                                if (m_CurrentConfig.Verbose != 0) log.Verbose = m_CurrentConfig.Verbose;
                                if (m_CurrentConfig.Other != 0) log.Other = m_CurrentConfig.Other;
                                if (m_CurrentConfig.Total != 0) log.Total = m_CurrentConfig.Total;
                            } catch (ArgumentOutOfRangeException ex) {
                                log = new SimplePrioMemoryLog {
                                    new LogEntry(TraceEventType.Warning, 0,
                                        $"Initializing ILogger {categoryName} defaults due to exception {ex.Message}") {
                                        Source = categoryName,
                                        DateTime = DateTime.Now
                                    }
                                };
                            }
                        }
                        m_LogCollection = log;
                        m_MemoryLog = new MemoryLogDump(m_LogCollection);

                        string message = string.Format("Initialized: C={0}; E={1}; W={2}; I={3}; V={4}; O={5}; T={6}",
                            log.Critical, log.Error, log.Warning, log.Info, log.Verbose, log.Other, log.Total);
                        LogEntry entry = new LogEntry(TraceEventType.Verbose, 0, message) {
                            Source = categoryName,
                            DateTime = DateTime.Now
                        };
                        m_MemoryLog.Add(entry);
                        CrashData.Instance.Providers.Add(m_MemoryLog);
                    }
                }
            }

            return m_Loggers.GetOrAdd(
                categoryName,
                name => {
                    return new MemoryLogger(name, m_MinLevel, m_MemoryLog);
                });
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            m_Loggers.Clear();
            m_OnChangeToken.Dispose();
            if (m_LogCollection != null) {
                m_LogCollection.Clear();
                CrashData.Instance.Providers.Remove(m_MemoryLog);
            }
        }
    }
}
