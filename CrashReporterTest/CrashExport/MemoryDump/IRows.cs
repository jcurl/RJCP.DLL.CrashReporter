namespace RJCP.Diagnostics.CrashExport.MemoryDump
{
    public interface IRows
    {
        IFields this[int index] { get; }

        int Count { get; }
    }
}
