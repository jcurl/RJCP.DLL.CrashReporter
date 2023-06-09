namespace RJCP.Diagnostics.Crash.Export.MemoryDump
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
#if NET45_OR_GREATER || NETCOREAPP
    using System.Threading.Tasks;
#endif

    public sealed class MemoryCrashDumpTable : DumpTable, IEnumerable<IFields>
    {
        private readonly object m_SyncRoot = new object();
        private readonly List<string> m_Fields = new List<string>();
        private readonly List<IFields> m_Rows = new List<IFields>();

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
        public override void DumpHeader(IEnumerable<string> header)
        {
            if (header == null) throw new ArgumentNullException(nameof(header));
            if (m_Rows.Count > 0) throw new InvalidOperationException("Dump header after dumping at least one row is invalid");
            if (IsDisposed) throw new ObjectDisposedException(nameof(MemoryCrashDumpTable));
            DumpHeaderInternal(header);
        }

        private void DumpHeaderInternal(IEnumerable<string> header)
        {
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
        public override void DumpRow(IDictionary<string, string> row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            if (IsDisposed) throw new ObjectDisposedException(nameof(MemoryCrashDumpTable));
            if (m_IsFlushed) throw new InvalidOperationException("Object flushed, writing is not allowed");
            DumpRowInternal(row);
        }

        private void DumpRowInternal(IDictionary<string, string> row)
        {
            lock (m_SyncRoot) {
                IFields newRow = new Fields(new Dictionary<string, string>());
                foreach (KeyValuePair<string, string> property in row) {
                    if (string.IsNullOrEmpty(property.Key)) throw new ArgumentException("Property provided that is null or empty", nameof(row));
                    if (!CheckField(property.Key)) {
                        string message = string.Format("Property '{0}' contains invalid character", property.Key);
                        throw new ArgumentException(message, nameof(row));
                    }
                    if (m_Fields.Count > 0 && !m_Fields.Contains(property.Key)) {
                        string message = string.Format("Field not defined: {0}", property.Key);
                        throw new ArgumentException(message, nameof(row));
                    }
                    newRow.Field.Add(property.Key, property.Value);
                }

                if (m_Fields.Count > 0) {
                    foreach (string field in m_Fields) {
                        if (!newRow.Field.ContainsKey(field)) {
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

        public override void Flush()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(MemoryCrashDumpTable));
            if (m_IsFlushed) throw new InvalidOperationException("Flushed twice, useless operation");

            /* Nothing to flush, as we're memory only */
            m_IsFlushed = true;
        }

#if NET45_OR_GREATER || NETCOREAPP
        public override Task DumpHeaderAsync(IEnumerable<string> header)
        {
            if (header == null) throw new ArgumentNullException(nameof(header));
            if (m_Rows.Count > 0) throw new InvalidOperationException("Dump header after dumping at least one row is invalid");
            if (IsDisposed) throw new ObjectDisposedException(nameof(MemoryCrashDumpTable));
            return Task.Run(() => { DumpHeaderInternal(header); });
        }

        public override Task DumpRowAsync(IDictionary<string, string> row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            if (IsDisposed) throw new ObjectDisposedException(nameof(MemoryCrashDumpTable));
            if (m_IsFlushed) throw new InvalidOperationException("Object flushed, writing is not allowed");
            return Task.Run(() => { DumpRowInternal(row); });
        }

        private readonly static Task Completed = Task.FromResult(true);    // .NET 4.6 and later has Task>Completed

        public override Task FlushAsync()
        {
            Flush();
            return Completed;
        }
#endif

        public string TableName { get; private set; }

        public string RowName { get; private set; }

        public IEnumerable<string> Headers { get { return m_Fields; } }

        private class Fields : IFields
        {
            public Fields(IDictionary<string, string> row) { Field = row; }

            public IDictionary<string, string> Field { get; private set; }
        }

        private class Rows : IRows
        {
            private readonly IList<IFields> m_Rows;

            public Rows(IList<IFields> rows) { m_Rows = rows; }

            public IFields this[int index] { get { return m_Rows[index]; } }

            public int Count { get { return m_Rows.Count; } }
        }

        public IRows Row { get { return new Rows(m_Rows); } }

        public IEnumerator<IFields> GetEnumerator()
        {
            return m_Rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
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
}
