namespace RJCP.Diagnostics.Crash.Export
{
    using System.IO;
#if NET45_OR_GREATER || NETSTANDARD
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// A factory interface for instantiating objects which can write dump files.
    /// </summary>
    public interface ICrashDumpFactory
    {
        /// <summary>
        /// The dumper recommended default file name.
        /// </summary>
        /// <value>
        /// The recommended file name to use creating a dump file using this factory. The recommended file name also
        /// contains the extension that is best used to interpret the file. If this property is
        /// <see cref="string.Empty"/>, the dumper ignores the file name and creates multiple files using the path
        /// provided as a directory (e.g. it might create a text file for each set of information required, such as
        /// CSV files).
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
        /// <example>
        /// <code language="csharp"><![CDATA[
        /// string baseDir = GetDumpBaseDirectory();    // Your code to get the directory for dumps for this process,
        ///                                             // based on the process name, PID, date/time, etc.
        /// string path = Path.Combine(baseDir, Crash.Data.CrashDumpFactory.FileName);
        /// using (ICrashDataDumpFile crashFile = Crash.Data.CrashDumpFactory.Create(path)) {
        ///   ...
        /// }
        /// ]]></code>
        /// </example>
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

#if NET45_OR_GREATER || NETSTANDARD
        /// <summary>
        /// Creates the dump from the given file name asynchronously.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        /// An <see cref="ICrashDataDumpFile"/> which can be given to dumpers implementing
        /// <see cref="ICrashDataExport"/>.
        /// </returns>
        /// <example>
        /// <code language="csharp"><![CDATA[
        /// string baseDir = GetDumpBaseDirectory();    // Your code to get the directory for dumps for this process,
        ///                                             // based on the process name, PID, date/time, etc.
        /// string path = Path.Combine(baseDir, Crash.Data.CrashDumpFactory.FileName);
        /// using (await ICrashDataDumpFile crashFile = Crash.Data.CrashDumpFactory.CreateAsync(path)) {
        ///   ...
        /// }
        /// ]]></code>
        /// </example>
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
