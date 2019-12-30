﻿namespace RJCP.Diagnostics.CrashExport
{
    using System;
    using System.Collections.Generic;
#if NET45
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// An object returned by <see cref="ICrashDataDumpFile.DumpTable(string, string)"/> for dumping properties to tables.
    /// </summary>
    public interface IDumpTable : IDisposable
    {
        /// <summary>
        /// Writes the header for the table.
        /// </summary>
        /// <param name="row">The row which also contains the list of headers.</param>
        void DumpHeader(DumpRow row);

        /// <summary>
        /// Writes the header for the table.
        /// </summary>
        /// <param name="header">The header.</param>
        void DumpHeader(IEnumerable<string> header);

        /// <summary>
        /// Writes the row of properties/values to the current table.
        /// </summary>
        /// <param name="row">The row, a collection of key/value pairs that should be written.</param>
        void DumpRow(DumpRow row);

        /// <summary>
        /// Writes the row of properties/values to the current table.
        /// </summary>
        /// <param name="row">The row, a collection of key/value pairs that should be written.</param>
        void DumpRow(IEnumerable<KeyValuePair<string, string>> row);

        /// <summary>
        /// Flushes all unwritten values to the output stream.
        /// </summary>
        void Flush();

#if NET45
        /// <summary>
        /// Writes the header for the table.
        /// </summary>
        /// <param name="row">The row which also contains the list of headers.</param>
        /// <returns>An awaitable task.</returns>
        Task DumpHeaderAsync(DumpRow row);

        /// <summary>
        /// Writes the header for the table asynchronously.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <returns>An awaitable task.</returns>
        Task DumpHeaderAsync(IEnumerable<string> header);

        /// <summary>
        /// Writes the row of properties/values to the current table asynchronously.
        /// </summary>
        /// <param name="row">The row, a collection of key/value pairs that should be written.</param>
        /// <returns>An awaitable task.</returns>
        Task DumpRowAsync(DumpRow row);

        /// <summary>
        /// Writes the row of properties/values to the current table asynchronously.
        /// </summary>
        /// <param name="row">The row, a collection of key/value pairs that should be written.</param>
        /// <returns>An awaitable task.</returns>
        Task DumpRowAsync(IEnumerable<KeyValuePair<string, string>> row);

        /// <summary>
        /// Flushes all unwritten values to the output stream asynchronously.
        /// </summary>
        /// <returns>An awaitable task.</returns>
        Task FlushAsync();
#endif
    }
}
