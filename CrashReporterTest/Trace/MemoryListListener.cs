namespace RJCP.Diagnostics.Trace
{
    public class MemoryListListener : MemoryTraceListener
    {
        public MemoryListListener() : base(new MemoryList()) { }
    }
}
