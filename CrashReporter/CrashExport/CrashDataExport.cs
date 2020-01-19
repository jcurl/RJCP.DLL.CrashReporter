namespace RJCP.Diagnostics.CrashExport
{
    using System;
    using System.Collections.Generic;
#if NET45
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// An abstract class to simplify the common case of dumping information to a dump file.
    /// </summary>
    /// <typeparam name="T">The data type for each row of data.</typeparam>
    public abstract class CrashDataExport<T> : ICrashDataExport
    {
        private DumpRow m_Row;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrashDataExport{T}"/> class.
        /// </summary>
        /// <param name="row">
        /// The row predefined with the headers, that will be passed while iterating over the dump data.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="row"/> is <see langword="null"/>.</exception>
        protected CrashDataExport(DumpRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            m_Row = row;
        }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        protected abstract string TableName { get; }

        /// <summary>
        /// Gets the name of the row.
        /// </summary>
        /// <value>
        /// The name of the row.
        /// </value>
        protected abstract string RowName { get; }

        /// <summary>
        /// An enumerable to get the objects that should be dumped.
        /// </summary>
        /// <returns>An enumerable object.</returns>
        protected abstract IEnumerable<T> GetRows();

        /// <summary>
        /// Updates the row given an item.
        /// </summary>
        /// <param name="item">The item returned from <see cref="GetRows()"/>.</param>
        /// <param name="row">The row that should be updated.</param>
        protected abstract void UpdateRow(T item, DumpRow row);

        /// <summary>
        /// Check if this object should dump.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if this instance is valid and data should be dumped; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        protected virtual bool IsValid() { return true; }

        /// <summary>
        /// Dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public void Dump(ICrashDataDumpFile dumpFile)
        {
            if (!IsValid()) return;

            using (IDumpTable table = dumpFile.DumpTable(TableName, RowName)) {
                table.DumpHeader(m_Row);
                foreach (T item in GetRows()) {
                    UpdateRow(item, m_Row);
                    table.DumpRow(m_Row);
                }
                table.Flush();
            }
        }

#if NET45
        /// <summary>
        /// Asynchronously dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        /// <returns>An awaitable task.</returns>
        public async Task DumpAsync(ICrashDataDumpFile dumpFile)
        {
            if (!IsValid()) return;

            using (IDumpTable table = await dumpFile.DumpTableAsync(TableName, RowName)) {
                await table.DumpHeaderAsync(m_Row);
                foreach (T item in GetRows()) {
                    UpdateRow(item, m_Row);
                    await table.DumpRowAsync(m_Row);
                }
                await table.FlushAsync();
            }
        }
#endif
    }
}
