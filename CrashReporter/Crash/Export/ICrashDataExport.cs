namespace RJCP.Diagnostics.Crash.Export
{
#if NET45_OR_GREATER || NETSTANDARD
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// Interface that dumpers must implement.
    /// </summary>
    public interface ICrashDataExport
    {
        /// <summary>
        /// Dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        void Dump(ICrashDataDumpFile dumpFile);

#if NET45_OR_GREATER || NETSTANDARD
        /// <summary>
        /// Asynchronously dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        /// <returns>An awaitable task.</returns>
        Task DumpAsync(ICrashDataDumpFile dumpFile);
#endif
    }
}
