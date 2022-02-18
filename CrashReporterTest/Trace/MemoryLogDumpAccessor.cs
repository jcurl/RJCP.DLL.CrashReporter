namespace RJCP.Diagnostics.Trace
{
    using CrashExport;
    using RJCP.CodeQuality;
#if NET45_OR_GREATER || NETCOREAPP
    using System.Threading.Tasks;
#endif

    public class MemoryLogDumpAccessor : AccessorBase, ICrashDataExport
    {
        public MemoryLogDumpAccessor(object instance)
            : base(new PrivateObject(instance)) { }

        public void Dump(ICrashDataDumpFile dumpFile)
        {
            Invoke(nameof(Dump), dumpFile);
        }

#if NET45_OR_GREATER || NETCOREAPP
        public Task DumpAsync(ICrashDataDumpFile dumpFile)
        {
            return (Task)Invoke(nameof(DumpAsync), dumpFile);
        }
#endif
    }
}
