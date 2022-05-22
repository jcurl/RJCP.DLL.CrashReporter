namespace RJCP.Diagnostics.CrashExport
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
#if NET45_OR_GREATER || NETSTANDARD
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// An abstract class to simplify the common case of dumping information to a dump file.
    /// </summary>
    /// <typeparam name="T">The data type for each row of data.</typeparam>
    public abstract class CrashDataExport<T> : ICrashDataExport
    {
        private readonly DumpRow m_Row;

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
        /// <returns>
        /// Returns <see langword="true"/> if the operation was successful and can be added to the dump file, else
        /// <see langword="false"/> that there was a problem and this row should be skipped.
        /// </returns>
        protected abstract bool UpdateRow(T item, DumpRow row);

        /// <summary>
        /// Check if this object should dump.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if this instance is valid and data should be dumped; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        protected virtual bool IsValid() { return true; }

        /// <summary>
        /// Attempts to run the delegate, returning a string.
        /// </summary>
        /// <param name="fieldAccess">The delegate accessing the dump data.</param>
        /// <returns>
        /// The result of the delegate <paramref name="fieldAccess"/>, else a string describing if an exception
        /// occurred.
        /// </returns>
        /// <remarks>
        /// The intent of this method is to easily wrap obtaining information which might throw an exception. If an
        /// exception is thrown, a simple string is returned indicating the error.
        /// </remarks>
        protected string GetField(Func<string> fieldAccess)
        {
            using (CrashReporter.SuppressFirstChanceException()) {
                try {
                    return fieldAccess();
                } catch (PlatformNotSupportedException) {
                    return "PlatformNotSupportedException";
                } catch (System.IO.FileNotFoundException) {
                    return "FileNotFoundException";
                } catch (System.IO.IOException ex) {
#if NET40
                    return string.Format("IOException: {0}", ex.Message);
#else
                    return string.Format("IOException: {0} (0x{1:x8})", ex.Message, ex.HResult);
#endif
                }
            }
        }

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
                    bool updated = false;
                    try {
                        updated = UpdateRow(item, m_Row);
                    } catch (Exception ex) {
                        Log.CrashLog.TraceEvent(TraceEventType.Error, "Couldn't dump row for {0}: {1}",
                            GetType().ToString(), ex.ToString());
                    }
                    if (updated) table.DumpRow(m_Row);
                }
                table.Flush();
            }
        }

#if NET45_OR_GREATER || NETSTANDARD
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
                    bool updated = false;
                    try {
                        updated = UpdateRow(item, m_Row);
                    } catch (Exception ex) {
                        Log.CrashLog.TraceEvent(TraceEventType.Error, "Couldn't dump row for {0}: {1}",
                            GetType().ToString(), ex.ToString());
                    }
                    if (updated) await table.DumpRowAsync(m_Row);
                }
                await table.FlushAsync();
            }
        }
#endif
    }
}
