﻿namespace RJCP.Diagnostics.Crash.Export.Xml
{
    using System;
    using System.IO;
#if NET45_OR_GREATER || NET6_0_OR_GREATER
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// Factory for creating an XML based crash dumper.
    /// </summary>
    public class XmlCrashDumpFactory : ICrashDumpFactory
    {
        /// <summary>
        /// The dumper recommended default file name.
        /// </summary>
        /// <value>
        /// The recommended file name to use creating a dump file using this factory. The recommended file name also
        /// contains the extension that is best used to interpret the file.
        /// </value>
        public string FileName { get { return XmlCrashDumper.DefaultFileName; } }

        /// <summary>
        /// Creates the dump from the given file name.
        /// </summary>
        /// <param name="fileName">Name of the file to create.</param>
        /// <returns>
        /// An <see cref="ICrashDataDumpFile"/> which can be given to dumpers implementing
        /// <see cref="ICrashDataExport"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// It is intended to call this method by choosing the directory where to store the dump, combined with the
        /// <see cref="FileName"/>. If the path given is a directory, the crash dump file will be created within that
        /// directory.
        /// </remarks>
        public ICrashDataDumpFile Create(string fileName)
        {
            ThrowHelper.ThrowIfNull(fileName);

            XmlCrashDumper dumper = new();
            try {
                dumper.CreateFile(fileName);
            } catch {
                dumper.Dispose();
                throw;
            }
            return dumper;
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
        /// <exception cref="ArgumentNullException">
        /// <paramref name="path"/> is <see langword="null"/>;
        /// <para>- or -</para>
        /// <paramref name="stream"/> is <see langword="null"/>.
        /// </exception>
        public ICrashDataDumpFile Create(Stream stream, string path)
        {
            ThrowHelper.ThrowIfNull(stream);
            ThrowHelper.ThrowIfNull(path);

            XmlCrashDumper dumper = new();
            try {
                dumper.CreateFile(stream, path);
            } catch {
                dumper.Dispose();
                throw;
            }
            return dumper;
        }

#if NET45_OR_GREATER || NET6_0_OR_GREATER
        /// <summary>
        /// Creates the dump from the given file name asynchronously.
        /// </summary>
        /// <param name="fileName">Name of the file to create.</param>
        /// <returns>
        /// An <see cref="ICrashDataDumpFile"/> which can be given to dumpers implementing
        /// <see cref="ICrashDataExport"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// It is intended to call this method by choosing the directory where to store the dump, combined with the
        /// <see cref="FileName"/>. If the path given is a directory, the crash dump file will be created within that
        /// directory.
        /// </remarks>
        public Task<ICrashDataDumpFile> CreateAsync(string fileName)
        {
            ThrowHelper.ThrowIfNull(fileName);
            return CreateAsyncInternal(fileName);
        }

        private static async Task<ICrashDataDumpFile> CreateAsyncInternal(string fileName)
        {
            XmlCrashDumper dumper = new();
            try {
                await dumper.CreateFileAsync(fileName).ConfigureAwait(false);
            } catch {
                dumper.Dispose();
                throw;
            }
            return dumper;
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
        /// <exception cref="ArgumentNullException">
        /// <paramref name="path"/> is <see langword="null"/>;
        /// <para>- or -</para>
        /// <paramref name="stream"/> is <see langword="null"/>.
        /// </exception>
        public Task<ICrashDataDumpFile> CreateAsync(Stream stream, string path)
        {
            ThrowHelper.ThrowIfNull(stream);
            ThrowHelper.ThrowIfNull(path);
            return CreateAsyncInternal(stream, path);
        }

        private static async Task<ICrashDataDumpFile> CreateAsyncInternal(Stream stream, string path)
        {
            XmlCrashDumper dumper = new();
            try {
                await dumper.CreateFileAsync(stream, path).ConfigureAwait(false);
            } catch {
                dumper.Dispose();
                throw;
            }
            return dumper;
        }
#endif
    }
}
