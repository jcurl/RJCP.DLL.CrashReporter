namespace RJCP.Diagnostics.Trace
{
    /// <summary>
    /// A simple memory trace logger.
    /// </summary>
    public sealed class SimplePrioMemoryTraceListener : MemoryTraceListener
    {
        private readonly SimplePrioMemoryLog m_MemoryLog;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimplePrioMemoryTraceListener"/> class.
        /// </summary>
        /// <remarks>
        /// You can change the default list size for logging by using the properties:
        /// <list type="bullet">
        /// <item>Critical</item>
        /// <item>Error</item>
        /// <item>Warning</item>
        /// <item>Information</item>
        /// <item>Verbose</item>
        /// <item>Other</item>
        /// <item>Total</item>
        /// </list>
        /// </remarks>
        public SimplePrioMemoryTraceListener() : this(new SimplePrioMemoryLog()) { }

        private SimplePrioMemoryTraceListener(SimplePrioMemoryLog memoryLog) : base(memoryLog)
        {
            m_MemoryLog = memoryLog;
        }

        private readonly string[] SupportedAttributes = new string[] {
            "Critical", "Error", "Warning", "Information", "Verbose", "Other", "Total"
        };

        /// <summary>
        /// Gets the custom attributes supported by the trace listener.
        /// </summary>
        /// <returns>
        /// A string array naming the custom attributes supported by the trace listener, or <see langword="null"/> if
        /// there are no custom attributes.
        /// </returns>
        protected override string[] GetSupportedAttributes()
        {
            return SupportedAttributes;
        }

        /// <summary>
        /// Initializes the trace data on the first call to log something.
        /// </summary>
        protected override void TraceInit()
        {
            if (int.TryParse(Attributes["Critical"], out int critical)) m_MemoryLog.Critical = critical;
            if (int.TryParse(Attributes["Error"], out int error)) m_MemoryLog.Error = error;
            if (int.TryParse(Attributes["Warning"], out int warning)) m_MemoryLog.Warning = warning;
            if (int.TryParse(Attributes["Information"], out int info)) m_MemoryLog.Info = info;
            if (int.TryParse(Attributes["Verbose"], out int verbose)) m_MemoryLog.Verbose = verbose;
            if (int.TryParse(Attributes["Other"], out int other)) m_MemoryLog.Other = other;
            if (int.TryParse(Attributes["Total"], out int total)) m_MemoryLog.Total = total;
        }
    }
}
