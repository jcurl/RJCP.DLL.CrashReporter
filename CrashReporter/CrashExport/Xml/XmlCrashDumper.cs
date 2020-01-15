﻿namespace RJCP.Diagnostics.CrashExport.Xml
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

        public void CreateFile(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            if (m_Writer != null) throw new InvalidOperationException("File is already created, cannot create twice");

            Path = System.IO.Path.GetDirectoryName(fileName);
            try {
                m_Writer = CreateFileInternal(fileName, false);
                m_Writer.WriteStartElement(RootName);
                m_IsFlushed = false;
            } catch (Exception ex) {
                Log.CrashLog.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "Error creating crash file: {0}", ex.ToString());
                Close();
                throw;
            }
        }

        public void CreateFile(Stream stream, string path)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (m_Writer != null) throw new InvalidOperationException("File is already created, cannot create twice");

            m_Stream = stream;
            Path = path;

            try {
                m_Writer = CreateFileInternal(stream, path, null, false);
                m_Writer.WriteStartElement(RootName);
                m_IsFlushed = false;
            } catch (Exception ex) {
                Log.CrashLog.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "Error creating crash stream: {0}", ex.ToString());
                Close();
                throw;
            }
        }

        private XmlWriter CreateFileInternal(string fileName, bool isAsync)
        {
            string directory = System.IO.Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            string styleSheetName = string.Format("{0}.xsl", System.IO.Path.GetFileNameWithoutExtension(fileName));
            string styleSheetPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(fileName), styleSheetName);
            CopyTransform(styleSheetPath);

            m_OwnsStream = true;
            m_Stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            return CreateFileInternal(m_Stream, directory, styleSheetName, isAsync);
        }

        private XmlWriter CreateFileInternal(Stream stream, string dirPath, string styleSheet, bool isAsync)
        {
            if (!Directory.Exists(dirPath)) {
                string message = string.Format("Directory '{0}' not found", dirPath);
                throw new DirectoryNotFoundException(message);
            }

            XmlWriter xmlWriter = XmlWriter.Create(stream, SaveXmlSettings(isAsync));
            if (styleSheet != null) {
                string stylesheetInstruction = string.Format("type=\"text/xsl\" href=\"{0}\"", styleSheet);
                xmlWriter.WriteProcessingInstruction("xml-stylesheet", stylesheetInstruction);
            }
            return xmlWriter;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1172:Unused method parameters should be removed",
            Justification = "Compiler conditional uses isAsync")]
        private XmlWriterSettings SaveXmlSettings(bool isAsync)
        {
            return new XmlWriterSettings {
#if NET45
                Async = isAsync,     // Defined in .NET 4.5 and later only.
#endif
                CloseOutput = false,
                ConformanceLevel = ConformanceLevel.Document,
                Encoding = Encoding.UTF8,
                Indent = true,
                IndentChars = "  ",
                NewLineOnAttributes = false
            };
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
            return new XmlDumpTable(rowName, m_Writer);
        }

        private bool m_IsFlushed = true;

        public void Flush()
        {
            if (!m_IsFlushed) {
                m_Writer.WriteEndElement();
                m_Writer.Flush();
                m_Stream.Flush();
                m_IsFlushed = true;
            }
        }

        private Stream GetStyleSheetResource()
        {
            string resource = Config.CrashReporter.AppConfig.Config.XmlCrashDumper.StyleSheetName;
            if (string.IsNullOrWhiteSpace(resource)) return GetDefaultStyleSheetResource();

            System.Reflection.Assembly assembly;
            string resourceName;
            int resourceClassPos = resource.LastIndexOf(',');
            if (resourceClassPos == -1) {
                // Assume that the user wants the executable (entry)
                assembly = System.Reflection.Assembly.GetEntryAssembly();
                resourceName = resource.Trim();
            } else {
                string assemblyName = resource.Substring(0, resourceClassPos).Trim();
                resourceName = resource.Substring(resourceClassPos + 1).Trim();

                assembly = GetAssemblyByName(assemblyName);
                if (assembly == null) return GetDefaultStyleSheetResource();
            }

            Stream stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return GetDefaultStyleSheetResource();
            return stream;
        }

        private System.Reflection.Assembly GetAssemblyByName(string name)
        {
            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                System.Reflection.AssemblyName assemblyName = assembly.GetName();
                if (name.Equals(assemblyName.FullName, StringComparison.OrdinalIgnoreCase)) return assembly;
                if (name.Equals(assemblyName.Name, StringComparison.OrdinalIgnoreCase)) return assembly;
            }
            return null;
        }

        private Stream GetDefaultStyleSheetResource()
        {
            return typeof(XmlCrashDumper).Assembly.GetManifestResourceStream("RJCP.Diagnostics.CrashExport.Xml.CrashDump.xsl");
        }

        private void CopyTransform(string outFileName)
        {
            using (Stream stream = GetStyleSheetResource())
            using (FileStream fileCopyStream = new FileStream(outFileName, FileMode.Create, FileAccess.Write, FileShare.None)) {
                stream.CopyTo(fileCopyStream);
            }
        }

#if NET45
        public Task CreateFileAsync(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            Path = System.IO.Path.GetDirectoryName(fileName);
            return CreateFileInternalAsync(fileName);
        }

        private async Task CreateFileInternalAsync(string fileName)
        {
            try {
                m_Writer = await Task.Run(() => { return CreateFileInternal(fileName, true); });
                await m_Writer.WriteStartElementAsync(null, RootName, null);
                m_IsFlushed = false;
            } catch (Exception ex) {
                Log.CrashLog.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "Error creating async crash file: {0}", ex.ToString());
                Close();
                throw;
            }
        }

        public Task CreateFileAsync(Stream stream, string path)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (m_Writer != null) throw new InvalidOperationException("File is already created, cannot create twice");

            m_Stream = stream;
            Path = path;

            return CreateFileInternalAsync(stream, path);
        }

        private async Task CreateFileInternalAsync(Stream stream, string path)
        {
            try {
                m_Writer = await Task.Run(() => { return CreateFileInternal(stream, path, null, true); });
                await m_Writer.WriteStartElementAsync(null, RootName, null);
                m_IsFlushed = false;
            } catch (Exception ex) {
                Log.CrashLog.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "Error creating async crash stream: {0}", ex.ToString());
                Close();
                throw;
            }
        }

        public Task<IDumpTable> DumpTableAsync(string tableName, string rowName)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Table Name may not be null or whitespace", nameof(tableName));
            if (string.IsNullOrWhiteSpace(rowName)) throw new ArgumentException("Row Name may not be null or whitespace", nameof(rowName));
            return DumpTableInternalAsync(tableName, rowName);
        }

        private async Task<IDumpTable> DumpTableInternalAsync(string tableName, string rowName)
        {
            await m_Writer.WriteStartElementAsync(null, tableName, null);

            // Update the variables after writing, so when we flush, we close that element. If writing the element would
            // raise an exception, we wouldn't close something that probably wasn't written.
            return new XmlDumpTable(rowName, m_Writer);
        }

        public async Task FlushAsync()
        {
            if (!m_IsFlushed) {
                await m_Writer.WriteEndElementAsync();
                await m_Writer.FlushAsync();
                await m_Stream.FlushAsync();
                m_IsFlushed = true;
            }
        }
#endif

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
            } catch (Exception ex) {
                Log.CrashLog.TraceEvent(System.Diagnostics.TraceEventType.Warning, 0, "XmlCrashDumper Disposing: Ignore exception {0}", ex.ToString());
                /* Ignore all errors when disposing, we might be disposing because of an error */
            }
        }
    }
}
