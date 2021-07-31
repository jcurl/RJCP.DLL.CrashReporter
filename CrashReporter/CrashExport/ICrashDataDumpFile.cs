namespace RJCP.Diagnostics.CrashExport
{
    using System;
#if NET45_OR_GREATER
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// An interface that describes how to write to a dump file.
    /// </summary>
    public interface ICrashDataDumpFile : IDisposable
    {
        /// <summary>
        /// Gets a value indicating if this instance can support writing blocks asynchronously.
        /// </summary>
        /// <value>
        /// Returns <see langword="true"/> if this instance is synchronous; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// If this object is synchronous (the value of this property is <see langword="true"/>), obtaining and using
        /// the <see cref="IDumpTable"/> returned by <see cref="DumpTable(string, string)"/> must by synchronous (the
        /// previous block must be disposed prior to obtaining a new one). For example, if the underlying implementing
        /// writes directly to disk, this would be typically a synchronous implementation.
        /// </remarks>
        bool IsSynchronous { get; }

        /// <summary>
        /// Gets the directory path to where data is being written.
        /// </summary>
        /// <value>
        /// The directory path to where data is being written. Other dump routines can use this path as a basis to add
        /// additional debugging information.
        /// </value>
        string Path { get; }

        /// <summary>
        /// Allocates a region for dumping information in a table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="rowName">The name for each individual row to be written.</param>
        /// <returns>
        /// An <see cref="IDumpTable"/> implementation that can be used to write properties for the block.
        /// </returns>
        IDumpTable DumpTable(string tableName, string rowName);

        /// <summary>
        /// Writes all pending data to disk. This should be called before disposing the object.
        /// </summary>
        void Flush();

#if NET45_OR_GREATER
        /// <summary>
        /// Allocates a region for dumping information in a table asynchronously.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="rowName">The name for each individual row to be written.</param>
        /// <returns>An awaitable task that returns a block which can be used to write properties of the block.</returns>
        Task<IDumpTable> DumpTableAsync(string tableName, string rowName);

        /// <summary>
        /// Asynchronously writes all pending data to disk. This should be called before disposing the object.
        /// </summary>
        /// <returns>An object that can be awaited on.</returns>
        Task FlushAsync();
#endif
    }
}
