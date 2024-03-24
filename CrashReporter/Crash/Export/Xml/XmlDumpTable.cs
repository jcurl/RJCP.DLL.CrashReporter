namespace RJCP.Diagnostics.Crash.Export.Xml
{
    using System.Collections.Generic;
    using System.Xml;
#if NET45_OR_GREATER || NET6_0_OR_GREATER
    using System.Threading.Tasks;
#endif

    internal sealed class XmlDumpTable : DumpTable
    {
        private readonly string m_RowName;
        private readonly XmlWriter m_Writer;
        private List<string> m_Fields;

        internal XmlDumpTable(string rowName, XmlWriter xmlWriter)
        {
            m_RowName = rowName;
            m_Writer = xmlWriter;
        }

        public override void DumpHeader(IEnumerable<string> header)
        {
            m_Fields = new List<string>(header);
        }

        public override void DumpRow(IDictionary<string, string> row)
        {
            if (m_IsFlushed) return;

            m_Writer.WriteStartElement(m_RowName);
            try {
                IEnumerable<string> fields = m_Fields ?? row.Keys;
                foreach (string field in fields) {
                    m_Writer.WriteAttributeString(field, XmlExtensions.SanitizeXml10(row[field]));
                }
            } finally {
                m_Writer.WriteEndElement();
            }
        }

        private bool m_IsFlushed;

        public override void Flush()
        {
            if (!m_IsFlushed) {
                m_Writer.WriteEndElement();
                m_IsFlushed = true;
            }
        }

#if NET45_OR_GREATER || NET6_0_OR_GREATER
        private readonly static Task Completed = Task.FromResult(true);

        public override Task DumpHeaderAsync(IEnumerable<string> header)
        {
            DumpHeader(header);
            return Completed;
        }

        public async override Task DumpRowAsync(IDictionary<string, string> row)
        {
            if (m_IsFlushed) return;

            await m_Writer.WriteStartElementAsync(null, m_RowName, null).ConfigureAwait(false);
            try {
                IEnumerable<string> fields = m_Fields ?? row.Keys;
                foreach (string field in fields) {
                    await m_Writer.WriteAttributeStringAsync(null, field, null, XmlExtensions.SanitizeXml10(row[field])).ConfigureAwait(false);
                }
            } finally {
                await m_Writer.WriteEndElementAsync().ConfigureAwait(false);
            }
        }

        public async override Task FlushAsync()
        {
            if (!m_IsFlushed) {
                await m_Writer.WriteEndElementAsync().ConfigureAwait(false);
                m_IsFlushed = true;
            }
        }
#endif

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                try {
                    Flush();
                } catch { /* Ignore errors when disposing */ }
            }
        }
    }
}
