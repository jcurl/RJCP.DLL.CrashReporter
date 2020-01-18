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
        /// Provides a recommended file name for the factory.
        /// </summary>
        /// <value>
        /// A recommended file name for creating a file with the factory. If the property is
        /// <see cref="string.Empty"/>, then the crash dumper creates multiple files in a directory.
        /// </value>
        string FileName { get; }

        /// <summary>
        /// Creates the dump from the given file name.
        /// </summary>
        /// <param name="fileName">
        /// Name of the file or directory. If the <see cref="FileName"/> is <see cref="string.Empty"/>, this is
        /// expected to be a directory, else it should be a file name.
        /// </param>
        /// <returns>
        /// An <see cref="ICrashDataDumpFile"/> which can be given to dumpers implementing
        /// <see cref="ICrashDataExport"/>.
        /// </returns>
        /// <remarks>
        /// It is intended to call this method by choosing the directory where to store the dump, combined with the
        /// <see cref="FileName"/>. If <see cref="FileName"/> is empty, this would result in the directory where to
        /// write multiple files to (and the dumper writes multiple files to that directory).
        /// </remarks>
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
        /// <remarks>
        /// It is intended that <paramref name="path"/> is called by choosing the directory where to store the dump,
        /// combined with the <see cref="FileName"/>. If <see cref="FileName"/> is empty, this would result in the
        /// directory where to write multiple files to (and the dumper writes multiple files to that directory).
        /// </remarks>
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
