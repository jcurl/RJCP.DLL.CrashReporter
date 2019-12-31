namespace RJCP.Diagnostics.Trace
{
    using System.Collections.Generic;

    /// <summary>
    /// An interface for logging trace events.
    /// </summary>
    public interface IMemoryLog : ICollection<LogEntry> { }
}
