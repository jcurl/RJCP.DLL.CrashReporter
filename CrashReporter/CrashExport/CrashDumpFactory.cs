namespace RJCP.Diagnostics.CrashExport
{
    using System;
    using System.IO;
#if NET45
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// A factory that knows how to create a dump file.
    /// </summary>
    public class CrashDumpFactory : ICrashDumpFactory
    {
        /// <summary>
        /// Creates the dump from the given file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        /// An <see cref="ICrashDataDumpFile" /> which can be given to dumpers implementing
        /// <see cref="ICrashDataExport" />.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public ICrashDataDumpFile Create(string fileName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes the dump file for the given stream.
        /// </summary>
        /// <param name="stream">The stream to write dump information to.</param>
        /// <param name="path">The path where additional dump files can be copied to.</param>
        /// <returns>
        /// An <see cref="ICrashDataDumpFile" /> which can be given to dumpers implementing
        /// <see cref="ICrashDataExport" />.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public ICrashDataDumpFile Create(Stream stream, string path)
        {
            throw new NotImplementedException();
        }

#if NET45
        /// <summary>
        /// Creates the dump from the given file name asynchronously.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        /// An <see cref="ICrashDataDumpFile" /> which can be given to dumpers implementing
        /// <see cref="ICrashDataExport" />.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<ICrashDataDumpFile> CreateAsync(string fileName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes the dump file for the given stream asynchronously.
        /// </summary>
        /// <param name="stream">The stream to write dump information to.</param>
        /// <param name="path">The path where additional dump files can be copied to.</param>
        /// <returns>
        /// An <see cref="ICrashDataDumpFile" /> which can be given to dumpers implementing
        /// <see cref="ICrashDataExport" />.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<ICrashDataDumpFile> CreateAsync(Stream stream, string path)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
