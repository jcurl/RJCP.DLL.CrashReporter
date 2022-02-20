namespace RJCP.Diagnostics.Logging
{
    /// <summary>
    /// The .NET Core Logging Configuration for the SimplePrioMemoryLog.
    /// </summary>
    public sealed class SimplePrioMemoryLogConfig
    {
        /// <summary>
        /// Gets or sets the minimum number of allowed critical log entries in the memory buffer.
        /// </summary>
        /// <value>The minimum number of allowed critical log entries in the memory buffer.</value>
        public int Critical { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of allowed error log entries in the memory buffer.
        /// </summary>
        /// <value>The minimum number of allowed error log entries in the memory buffer.</value>
        public int Error { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of allowed warning log entries in the memory buffer.
        /// </summary>
        /// <value>The minimum number of allowed warning log entries in the memory buffer.</value>
        public int Warning { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of allowed informational log entries in the memory buffer.
        /// </summary>
        /// <value>The minimum number of allowed informational log entries in the memory buffer.</value>
        public int Info { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of allowed verbose log entries (equivalent to the debug type) in the memory
        /// buffer.
        /// </summary>
        /// <value>The minimum number of allowed verbose log entries in the memory buffer.</value>
        public int Verbose { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of allowed other log entries in the memory buffer.
        /// </summary>
        /// <value>The minimum number of allowed other log entries in the memory buffer.</value>
        public int Other { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of allowed log entries in the memory buffer.
        /// </summary>
        /// <value>The maximum number of allowed log entries in the memory buffer.</value>
        public int Total { get; set; }
    }
}
