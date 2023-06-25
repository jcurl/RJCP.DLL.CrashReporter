namespace RJCP.Diagnostics.Crash.Dumpers
{
    using Export;
    using RJCP.CodeQuality;
#if !NET40_LEGACY
    using System.Threading.Tasks;
#endif

    public class AssemblyDumpAccessor : AccessorBase, ICrashDataExport
    {
        private const string AssemblyName = "RJCP.Diagnostics.CrashReporter";
        private const string TypeName = "RJCP.Diagnostics.Crash.Dumpers.AssemblyDump";
        public static readonly PrivateType AccType = new PrivateType(AssemblyName, TypeName);

        public AssemblyDumpAccessor() : base(AccType) { }

        /// <summary>
        /// Dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public void Dump(ICrashDataDumpFile dumpFile)
        {
            Invoke(nameof(Dump), dumpFile);
        }

#if !NET40_LEGACY
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
