namespace RJCP.Diagnostics.Trace
{
    /// <summary>
    /// A simple memory trace logger.
    /// </summary>
    public sealed class SimplePrioMemoryTraceListener : MemoryTraceListener
    {
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
        public SimplePrioMemoryTraceListener() : base(new SimplePrioMemoryLog()) { }

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
            if (int.TryParse(Attributes["Critical"], out int critical)) ((SimplePrioMemoryLog)MemoryLog).Critical = critical;
            if (int.TryParse(Attributes["Error"], out int error)) ((SimplePrioMemoryLog)MemoryLog).Error = error;
            if (int.TryParse(Attributes["Warning"], out int warning)) ((SimplePrioMemoryLog)MemoryLog).Warning = warning;
            if (int.TryParse(Attributes["Information"], out int info)) ((SimplePrioMemoryLog)MemoryLog).Info = info;
            if (int.TryParse(Attributes["Verbose"], out int verbose)) ((SimplePrioMemoryLog)MemoryLog).Verbose = verbose;
            if (int.TryParse(Attributes["Other"], out int other)) ((SimplePrioMemoryLog)MemoryLog).Other = other;
            if (int.TryParse(Attributes["Total"], out int total)) ((SimplePrioMemoryLog)MemoryLog).Total = total;
        }
    }
}
