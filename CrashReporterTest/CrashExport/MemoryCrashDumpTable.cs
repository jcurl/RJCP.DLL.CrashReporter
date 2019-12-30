namespace RJCP.Diagnostics.CrashExport
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class MemoryCrashDumpTable : IDumpTable
    {
        private readonly object m_SyncRoot = new object();
        private List<string> m_Fields = new List<string>();
        private List<Dictionary<string, string>> m_Rows = new List<Dictionary<string, string>>();

        internal MemoryCrashDumpTable(string tableName, string rowName)
        {
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));
            if (rowName == null) throw new ArgumentNullException(nameof(rowName));
            TableName = tableName;
            RowName = rowName;
        }

        /// <summary>
        /// Writes the header for the table.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="header"/> may not be <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Dump header after dumping at least one row is invalid.
        /// </exception>
        /// <exception cref="ObjectDisposedException"><see cref="MemoryCrashDumpTable"/> is disposed.</exception>
        /// <exception cref="ArgumentException">
        /// Field provided that is null or empty;
        /// <para>- or -</para>
        /// Field contains invalid character;
        /// <para>- or -</para>
        /// Field specified multiple times.
        /// </exception>
        /// <remarks>
        /// A header can specify a header row containing the fields. This implementation also checks that if a header is
        /// provided, then all fields are always provided.
        /// </remarks>
        public void DumpHeader(IEnumerable<string> header)
        {
            if (header == null) throw new ArgumentNullException(nameof(header));
            if (m_Rows.Count > 0) throw new InvalidOperationException("Dump header after dumping at least one row is invalid");
            if (IsDisposed) throw new ObjectDisposedException(nameof(MemoryCrashDumpTable));

            lock (m_SyncRoot) {
                HashSet<string> fields = new HashSet<string>();
                foreach (string field in header) {
                    if (string.IsNullOrEmpty(field)) throw new ArgumentException("Field provided that is null or empty", nameof(header));
                    if (!CheckField(field)) {
                        string message = string.Format("Field '{0}' contains invalid character", field);
                        throw new ArgumentException(message, nameof(header));
                    }
                    if (fields.Contains(field)) {
                        string message = string.Format("Field '{0}' specified multiple times", field);
                        throw new ArgumentException(message);
                    }
                    fields.Add(field);
                    m_Fields.Add(field);
                }
                if (fields.Count == 0) throw new ArgumentException("Empty header provided");
            }
        }

        /// <summary>
        /// Writes the row of properties/values to the current table.
        /// </summary>
        /// <param name="row">The row, a collection of key/value pairs that should be written.</param>
        /// <exception cref="ArgumentNullException"><paramref name="row"/> may not be <see langword="null"/>.</exception>
        /// <exception cref="ObjectDisposedException">MemoryCrashDumpTable</exception>
        /// <exception cref="ArgumentException">
        /// Duplicate property provided;
        /// <para>- or -</para>
        /// Property provided that is null or empty;
        /// <para>- or -</para>
        /// Property contains invalid character;
        /// <para>- or -</para>
        /// Header row is present, and a field is given that was not defined in the header row;
        /// <para>- or -</para>
        /// Header row is present, and a field is missing from the row.
        /// </exception>
        /// <exception cref="InvalidOperationException">Object flushed, writing is not allowed;</exception>
        /// <remarks>
        /// This implementation can assist to ensure that dumps write proper and complete information to the table.
        /// </remarks>
        public void DumpRow(IEnumerable<KeyValuePair<string, string>> row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            if (IsDisposed) throw new ObjectDisposedException(nameof(MemoryCrashDumpTable));
            if (m_IsFlushed) throw new InvalidOperationException("Object flushed, writing is not allowed");
            lock (m_SyncRoot) {
                Dictionary<string, string> newRow = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> property in row) {
                    if (string.IsNullOrEmpty(property.Key)) throw new ArgumentException("Property provided that is null or empty", nameof(row));
                    if (!CheckField(property.Key)) {
                        string message = string.Format("Property '{0}' contains invalid character", property.Key);
                        throw new ArgumentException(message, nameof(row));
                    }
                    if (newRow.ContainsKey(property.Key)) {
                        string message = string.Format("Duplicate property: {0}", property.Key);
                        throw new ArgumentException(message, nameof(row));
                    }
                    if (m_Fields.Count > 0 && !m_Fields.Contains(property.Key)) {
                        string message = string.Format("Field not defined: {0}", property.Key);
                        throw new ArgumentException(message, nameof(row));
                    }
                    newRow.Add(property.Key, property.Value);
                }

                if (m_Fields.Count > 0) {
                    foreach (string field in m_Fields) {
                        if (!newRow.ContainsKey(field)) {
                            string message = string.Format("Missing property for field: {0}", field);
                            throw new ArgumentException(message, nameof(row));
                        }
                    }
                }

                m_Rows.Add(newRow);
            }
        }

        private readonly char[] AllowedFieldChars = new char[] { };

        private bool CheckField(string field)
        {
            foreach (char c in field) {
                if (!char.IsLetterOrDigit(c) && !AllowedFieldChars.Contains(c)) return false;
            }
            return true;
        }

        bool m_IsFlushed;

        public void Flush()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(MemoryCrashDumpTable));
            if (m_IsFlushed) throw new InvalidOperationException("Flushed twice, useless operation");

            /* Nothing to flush, as we're memory only */
            m_IsFlushed = true;
        }

        public string TableName { get; private set; }

        public string RowName { get; private set; }

        /// <summary>
        /// Gets the set of properties for a specific row in the table.
        /// </summary>
        /// <value>
        /// The set of properties for the specific row requested in the table.
        /// </value>
        /// <param name="row">The row id to get, from 0 to <see cref="Count"/> - 1.</param>
        /// <returns>The set of properties for the specific row requested in the table.</returns>
        public IDictionary<string, string> this[int row]
        {
            get
            {
                lock (m_SyncRoot) {
                    return m_Rows[row];
                }
            }
        }

        /// <summary>
        /// Gets the number of rows in this table.
        /// </summary>
        /// <value>
        /// The number of rows in this table.
        /// </value>
        public int Count
        {
            get
            {
                lock (m_SyncRoot) {
                    return m_Rows.Count;
                }
            }
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (!m_IsFlushed) {
                Console.WriteLine("Dispose called on memory table '{0}' without flushing. " +
                    "Recommend flushing. Not critical, but can lead to synchronous flush",
                    TableName);
            }
            IsDisposed = true;
            m_IsFlushed = true;
        }
    }
}
