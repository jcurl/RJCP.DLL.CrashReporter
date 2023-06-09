namespace RJCP.Diagnostics.Crash.Export.MemoryDump
{
    public interface IRows
    {
        IFields this[int index] { get; }

        int Count { get; }
    }
}
