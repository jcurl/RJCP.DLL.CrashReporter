namespace RJCP.Diagnostics.Crash.Export
{
    using System;
#if NET45_OR_GREATER || NET6_0_OR_GREATER
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// An interface that describes how to write to a dump file.
    /// </summary>
    public interface ICrashDataDumpFile : IDisposable
    {
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

#if NET45_OR_GREATER || NET6_0_OR_GREATER
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
