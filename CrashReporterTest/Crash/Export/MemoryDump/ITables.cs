namespace RJCP.Diagnostics.Crash.Export.MemoryDump
{
    using System.Collections.Generic;

    public interface ITables
    {
        IList<MemoryCrashDumpTable> Table { get; }
    }
}
