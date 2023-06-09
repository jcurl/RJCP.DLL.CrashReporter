namespace RJCP.Diagnostics.Crash.Export
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines a row in a dump table, that is easy to use with <see cref="IDumpTable"/>.
    /// </summary>
    public class DumpRow
    {
        private readonly List<string> m_Header;
        private readonly Dictionary<string, string> m_Row;

        /// <summary>
        /// Initializes a new instance of the <see cref="DumpRow"/> class.
        /// </summary>
        /// <param name="header">The headers for each column in the table.</param>
        /// <exception cref="ArgumentNullException"><paramref name="header"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// Header must have at least one field;
        /// <para>- or -</para>
        /// Header has a duplicate entry.
        /// </exception>
        public DumpRow(params string[] header)
        {
            if (header == null) throw new ArgumentNullException(nameof(header));
            if (header.Length == 0) throw new ArgumentException("Header must have at least one field", nameof(header));

            m_Header = new List<string>(header.Length);
            m_Row = new Dictionary<string, string>();
            foreach (string field in header) {
                if (m_Row.ContainsKey(field)) {
                    string message = string.Format("Header '{0} defined multiple times", field);
                    throw new ArgumentException(message, nameof(header));
                }
                m_Header.Add(field);
                m_Row.Add(field, string.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the value of a particular field for this row.
        /// </summary>
        /// <value>The value of a particular field for this row.</value>
        /// <param name="key">The name of the column (defined in the headers).</param>
        /// <returns>The value of the particular field for this row.</returns>
        public string this[string key]
        {
            get { return m_Row[key]; }
            set { m_Row[key] = value; }
        }

        /// <summary>
        /// Clears this row, so all entries are <see cref="String.Empty"/>.
        /// </summary>
        public void Clear()
        {
            foreach (string key in m_Header) {
                m_Row[key] = string.Empty;
            }
        }

        /// <summary>
        /// Gets the header row.
        /// </summary>
        /// <returns>An enumerable for all elements in the header.</returns>
        public IEnumerable<string> GetHeader()
        {
            return m_Header;
        }

        /// <summary>
        /// Gets the row as key/value pairs.
        /// </summary>
        /// <returns>
        /// A key/value pair, where the key is the name of the column, the value is the value assigned for this field.
        /// </returns>
        public IDictionary<string, string> GetRow()
        {
            return m_Row;
        }
    }
}
