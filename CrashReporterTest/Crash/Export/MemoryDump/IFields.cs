namespace RJCP.Diagnostics.Crash.Export.MemoryDump
{
    using System.Collections.Generic;

    public interface IFields
    {
        IDictionary<string, string> Field { get; }
    }
}
