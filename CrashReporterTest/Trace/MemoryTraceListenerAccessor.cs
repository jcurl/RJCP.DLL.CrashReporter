namespace RJCP.Diagnostics.Trace
{
    using System.Reflection;
    using RJCP.CodeQuality;

    public class MemoryTraceListenerAccessor : AccessorBase
    {
        public MemoryTraceListenerAccessor(MemoryTraceListener instance)
            : base(new PrivateObject(instance, new PrivateType(typeof(MemoryTraceListener))))
        {
            BindingFlags |= BindingFlags.NonPublic;
        }

        public MemoryLogDumpAccessor MemoryLogDump
        {
            get
            {
                object memoryLog = GetFieldOrProperty("m_MemoryLog");
                return new MemoryLogDumpAccessor(memoryLog);
            }
        }
    }
}
