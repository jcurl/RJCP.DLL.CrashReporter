namespace RJCP.Diagnostics.CrashExport.MemoryDump
{
    using System.Collections.Generic;

    public interface IFields
    {
        IDictionary<string, string> Field { get; }
    }
}
