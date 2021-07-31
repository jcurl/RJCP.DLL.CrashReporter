namespace RJCP.Diagnostics.CrashExport
{
    using System.IO;
#if NET45_OR_GREATER
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// A factory that knows how to create a dump file.
    /// </summary>
    public class CrashDumpFactory : ICrashDumpFactory
    {
        private readonly ICrashDumpFactory m_XmlFactory = new Xml.XmlCrashDumpFactory();

        /// <summary>
        /// Provides a recommended file name for the factory.
        /// </summary>
        /// <value>A recommended file name for creating a file with the factory.</value>
        public string FileName { get { return m_XmlFactory.FileName; } }

        /// <summary>
        /// Creates the dump from the given file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        /// An <see cref="ICrashDataDumpFile"/> which can be given to dumpers implementing
        /// <see cref="ICrashDataExport"/>.
        /// </returns>
        public ICrashDataDumpFile Create(string fileName)
        {
            return m_XmlFactory.Create(fileName);
        }

        /// <summary>
        /// Initializes the dump file for the given stream.
        /// </summary>
        /// <param name="stream">The stream to write dump information to.</param>
        /// <param name="path">The path where additional dump files can be copied to.</param>
        /// <returns>
        /// An <see cref="ICrashDataDumpFile"/> which can be given to dumpers implementing
        /// <see cref="ICrashDataExport"/>.
        /// </returns>
        public ICrashDataDumpFile Create(Stream stream, string path)
        {
            return m_XmlFactory.Create(stream, path);
        }

#if NET45_OR_GREATER
        /// <summary>
        /// Creates the dump from the given file name asynchronously.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        /// An <see cref="ICrashDataDumpFile"/> which can be given to dumpers implementing
        /// <see cref="ICrashDataExport"/>.
        /// </returns>
        public Task<ICrashDataDumpFile> CreateAsync(string fileName)
        {
            return m_XmlFactory.CreateAsync(fileName);
        }

        /// <summary>
        /// Initializes the dump file for the given stream asynchronously.
        /// </summary>
        /// <param name="stream">The stream to write dump information to.</param>
        /// <param name="path">The path where additional dump files can be copied to.</param>
        /// <returns>
        /// An <see cref="ICrashDataDumpFile"/> which can be given to dumpers implementing
        /// <see cref="ICrashDataExport"/>.
        /// </returns>
        public Task<ICrashDataDumpFile> CreateAsync(Stream stream, string path)
        {
            return m_XmlFactory.CreateAsync(stream, path);
        }
#endif
    }
}
