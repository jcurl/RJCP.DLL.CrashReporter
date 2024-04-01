namespace RJCP.Diagnostics.Crash.Export.MemoryDump
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
#if !NET40_LEGACY
    using System.Threading.Tasks;
#endif

    public sealed class MemoryCrashDumpTable : DumpTable, IEnumerable<IFields>
    {
        private readonly object m_SyncRoot = new();
        private readonly List<string> m_Fields = new();
        private readonly List<IFields> m_Rows = new();

        internal MemoryCrashDumpTable(string tableName, string rowName)
        {
            ThrowHelper.ThrowIfNull(tableName);
            ThrowHelper.ThrowIfNull(rowName);
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
            ThrowHelper.ThrowIfDisposed(IsDisposed, this);
            ThrowHelper.ThrowIfNull(header);
            if (m_Rows.Count > 0) throw new InvalidOperationException("Dump header after dumping at least one row is invalid");
            DumpHeaderInternal(header);
        }

        private void DumpHeaderInternal(IEnumerable<string> header)
        {
            lock (m_SyncRoot) {
                HashSet<string> fields = new();
                foreach (string field in header) {
                    if (string.IsNullOrEmpty(field)) throw new ArgumentException("Field provided that is null or empty", nameof(header));
                    if (!CheckField(field)) {
                        throw new ArgumentException($"Field '{field}' contains invalid character", nameof(header));
                    }
                    if (fields.Contains(field)) {
                        throw new ArgumentException($"Field '{field}' specified multiple times");
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
            ThrowHelper.ThrowIfDisposed(IsDisposed, this);
            ThrowHelper.ThrowIfNull(row);
            if (m_IsFlushed) throw new InvalidOperationException("Object flushed, writing is not allowed");
            DumpRowInternal(row);
        }

        private void DumpRowInternal(IDictionary<string, string> row)
        {
            lock (m_SyncRoot) {
                IFields newRow = new Fields(new Dictionary<string, string>());
                foreach (KeyValuePair<string, string> property in row) {
                    if (string.IsNullOrEmpty(property.Key)) throw new ArgumentException("Property provided that key is null or empty", nameof(row));
                    if (!CheckField(property.Key)) {
                        throw new ArgumentException($"Property '{property.Key}' contains invalid character", nameof(row));
                    }
                    if (m_Fields.Count > 0 && !m_Fields.Contains(property.Key)) {
                        throw new ArgumentException($"Field not defined: {property.Key}", nameof(row));
                    }
                    newRow.Field.Add(property.Key, property.Value);
                }

                if (m_Fields.Count > 0) {
                    foreach (string field in m_Fields) {
                        if (!newRow.Field.ContainsKey(field)) {
                            throw new ArgumentException($"Missing property for field: {field}", nameof(row));
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
            ThrowHelper.ThrowIfDisposed(IsDisposed, this);
            if (m_IsFlushed) throw new InvalidOperationException("Flushed twice, useless operation");

            /* Nothing to flush, as we're memory only */
            m_IsFlushed = true;
        }

#if !NET40_LEGACY
        public override Task DumpHeaderAsync(IEnumerable<string> header)
        {
            ThrowHelper.ThrowIfDisposed(IsDisposed, this);
            ThrowHelper.ThrowIfNull(header);
            if (m_Rows.Count > 0) throw new InvalidOperationException("Dump header after dumping at least one row is invalid");
            return Task.Run(() => { DumpHeaderInternal(header); });
        }

        public override Task DumpRowAsync(IDictionary<string, string> row)
        {
            ThrowHelper.ThrowIfDisposed(IsDisposed, this);
            ThrowHelper.ThrowIfNull(row);
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
