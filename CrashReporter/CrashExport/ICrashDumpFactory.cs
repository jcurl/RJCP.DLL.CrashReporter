namespace RJCP.Diagnostics.CrashExport
{
    using System.IO;
#if NET45
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// A factory interface for instantiating objects which can write dump files.
    /// </summary>
    public interface ICrashDumpFactory
    {
        /// <summary>
        /// Creates the dump from the given file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        /// An <see cref="ICrashDataDumpFile"/> which can be given to dumpers implementing
        /// <see cref="ICrashDataExport"/>.
        /// </returns>
        ICrashDataDumpFile Create(string fileName);

        /// <summary>
        /// Initializes the dump file for the given stream.
        /// </summary>
        /// <param name="stream">The stream to write dump information to.</param>
        /// <param name="path">The path where additional dump files can be copied to.</param>
        /// <returns>
        /// An <see cref="ICrashDataDumpFile"/> which can be given to dumpers implementing
        /// <see cref="ICrashDataExport"/>.
        /// </returns>
        ICrashDataDumpFile Create(Stream stream, string path);

#if NET45
        /// <summary>
        /// Creates the dump from the given file name asynchronously.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        /// An <see cref="ICrashDataDumpFile"/> which can be given to dumpers implementing
        /// <see cref="ICrashDataExport"/>.
        /// </returns>
        Task<ICrashDataDumpFile> CreateAsync(string fileName);

        /// <summary>
        /// Initializes the dump file for the given stream asynchronously.
        /// </summary>
        /// <param name="stream">The stream to write dump information to.</param>
        /// <param name="path">The path where additional dump files can be copied to.</param>
        /// <returns>
        /// An <see cref="ICrashDataDumpFile"/> which can be given to dumpers implementing
        /// <see cref="ICrashDataExport"/>.
        /// </returns>
        Task<ICrashDataDumpFile> CreateAsync(Stream stream, string path);
#endif
    }
}
