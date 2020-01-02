namespace RJCP.Diagnostics.CrashExport.MemoryDump
{
    using System.Collections.Generic;

    public interface ITables
    {
        IList<MemoryCrashDumpTable> Table { get; }
    }
}
