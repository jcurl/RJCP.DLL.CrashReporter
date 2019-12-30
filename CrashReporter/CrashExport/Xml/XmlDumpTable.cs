namespace RJCP.Diagnostics.CrashExport.Xml
{
    using System.Collections.Generic;
    using System.Xml;
#if NET45
    using System.Threading.Tasks;
#endif

    internal sealed class XmlDumpTable : DumpTable
    {
        private string m_RowName;
        private XmlWriter m_Writer;
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
            m_Writer.WriteStartElement(m_RowName);
            try {
                IEnumerable<string> fields = m_Fields ?? row.Keys;
                foreach(string field in fields) {
                    m_Writer.WriteAttributeString(field, row[field]);
                }
            } finally {
                m_Writer.WriteEndElement();
            }
        }

        public override void Flush()
        {
            /* Nothing to flush */
        }

#if NET45
        private static Task Completed = Task.FromResult(true);

        public override Task DumpHeaderAsync(IEnumerable<string> header)
        {
            DumpHeader(header);
            return Completed;
        }

        public async override Task DumpRowAsync(IDictionary<string, string> row)
        {
            await m_Writer.WriteStartElementAsync(null, m_RowName, null);
            try {
                IEnumerable<string> fields = m_Fields ?? row.Keys;
                foreach (string field in fields) {
                    await m_Writer.WriteAttributeStringAsync(null, field, null, row[field]);
                }
            } finally {
                await m_Writer.WriteEndElementAsync();
            }
        }

        public override Task FlushAsync()
        {
            /* Nothing to flush */
            return Completed;
        }
#endif
    }
}
