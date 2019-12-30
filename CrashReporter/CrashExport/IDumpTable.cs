namespace RJCP.Diagnostics.CrashExport
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An object returned by <see cref="ICrashDataDumpFile.DumpTable(string, string)"/> for dumping properties to tables.
    /// </summary>
    public interface IDumpTable : IDisposable
    {
        /// <summary>
        /// Writes the header for the table.
        /// </summary>
        /// <param name="header">The header.</param>
        void DumpHeader(IEnumerable<string> header);

        /// <summary>
        /// Writes the row of properties/values to the current table.
        /// </summary>
        /// <param name="row">The row, a collection of key/value pairs that should be written.</param>
        void DumpRow(IEnumerable<KeyValuePair<string, string>> row);

        /// <summary>
        /// Flushes all unwritten values to the output stream.
        /// </summary>
        void Flush();
    }
}
