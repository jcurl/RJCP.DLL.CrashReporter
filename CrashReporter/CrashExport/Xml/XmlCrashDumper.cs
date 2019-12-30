namespace RJCP.Diagnostics.CrashExport.Xml
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;
#if NET45
    using System.Threading.Tasks;
#endif

    internal sealed class XmlCrashDumper : ICrashDataDumpFile
    {
        private const string RootName = "DiagnosticDump";

        private bool m_OwnsStream;
        private Stream m_Stream;
        private XmlWriter m_Writer;

        private string m_TableName;

        internal XmlCrashDumper() { }

        public void CreateFile(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            if (m_Writer != null) throw new InvalidOperationException("File is already created, cannot create twice");

            try {
                string directory = System.IO.Path.GetDirectoryName(fileName);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                m_OwnsStream = true;
                m_Stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
                CreateFile(m_Stream, System.IO.Path.GetDirectoryName(fileName));
            } catch {
                Close();
                throw;
            }
        }

        public void CreateFile(Stream stream, string path)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (m_Writer != null) throw new InvalidOperationException("File is already created, cannot create twice");

            if (!Directory.Exists(path)) {
                string message = string.Format("Directory '{0}' not found", path);
                throw new DirectoryNotFoundException(message);
            }

            m_Stream = stream;
            try {
                m_Writer = XmlWriter.Create(stream, SaveXmlSettings());
                Path = path;

                WriteRoot(m_Writer);
            } catch {
                Close();
                throw;
            }
        }

        private XmlWriterSettings SaveXmlSettings()
        {
            return new XmlWriterSettings {
                CloseOutput = false,
                ConformanceLevel = ConformanceLevel.Document,
                Encoding = Encoding.UTF8,
                Indent = true,
                IndentChars = "  ",
                NewLineOnAttributes = false
            };
        }

        private void WriteRoot(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement(RootName);
            m_IsFlushed = false;
        }

        /// <summary>
        /// Gets a value indicating if this instance can support writing blocks asynchronously.
        /// </summary>
        /// <value>
        /// Returns <see langword="true"/> if this instance is synchronous; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// This is a synchronous implementation - it writes directly to the XML file when dumping rows, rather than
        /// caching them first.
        /// </remarks>
        public bool IsSynchronous { get { return true; } }

        public string Path { get; private set; }

        public IDumpTable DumpTable(string tableName, string rowName)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Table Name may not be null or whitespace", nameof(tableName));
            if (string.IsNullOrWhiteSpace(rowName)) throw new ArgumentException("Row Name may not be null or whitespace", nameof(rowName));

            m_Writer.WriteStartElement(tableName);

            // Update the variables after writing, so when we flush, we close that element. If writing the element would
            // raise an exception, we wouldn't close something that probably wasn't written.
            m_TableName = tableName;
            return new XmlDumpTable(rowName, m_Writer);
        }

        private bool m_IsFlushed = true;

        public void Flush()
        {
            if (!m_IsFlushed) {
                if (m_TableName != null) m_Writer.WriteEndElement();

                m_Writer.WriteEndElement();
                m_Writer.Flush();
                m_Stream.Flush();
                m_IsFlushed = true;
            }
        }

        private void Close()
        {
            if (!m_IsFlushed) Flush();

            if (m_Writer != null) ((IDisposable)m_Writer).Dispose();
            m_Writer = null;

            if (m_OwnsStream && m_Stream != null) m_Stream.Dispose();
            m_Stream = null;
        }

        public void Dispose()
        {
            try {
                Close();
            } catch { /* Ignore all errors when disposing, we might be disposing because of an error */ }
        }

#if NET45
        public Task CreateFileAsync(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));

            throw new NotImplementedException();
        }

        public Task CreateFileAsync(Stream stream, string path)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (path == null) throw new ArgumentNullException(nameof(path));

            throw new NotImplementedException();
        }

        public Task<IDumpTable> DumpTableAsync(string tableName, string rowName)
        {
            throw new NotImplementedException();
        }

        public Task FlushAsync()
        {
            throw new NotImplementedException();
        }
#endif
    }
}
