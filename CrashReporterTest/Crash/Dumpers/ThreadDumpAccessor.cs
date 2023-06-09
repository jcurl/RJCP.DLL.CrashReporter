namespace RJCP.Diagnostics.Crash.Dumpers
{
    using Export;
    using RJCP.CodeQuality;
#if NET45_OR_GREATER || NETCOREAPP
    using System.Threading.Tasks;
#endif

    public class ThreadDumpAccessor : AccessorBase, ICrashDataExport
    {
        private const string AssemblyName = "RJCP.Diagnostics.CrashReporter";
        private const string TypeName = "RJCP.Diagnostics.Crash.Dumpers.ThreadDump";
        public static readonly PrivateType AccType = new PrivateType(AssemblyName, TypeName);

        public ThreadDumpAccessor() : base(AccType) { }

        /// <summary>
        /// Dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public void Dump(ICrashDataDumpFile dumpFile)
        {
            Invoke(nameof(Dump), dumpFile);
        }

#if NET45_OR_GREATER || NETCOREAPP
        /// <summary>
        /// Asynchronously dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        /// <returns>An awaitable task.</returns>
        public Task DumpAsync(ICrashDataDumpFile dumpFile)
        {
            return (Task)Invoke(nameof(DumpAsync), dumpFile);
        }
#endif
    }
}
