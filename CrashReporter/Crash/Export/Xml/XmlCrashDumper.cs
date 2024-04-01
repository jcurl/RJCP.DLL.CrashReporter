namespace RJCP.Diagnostics.Crash.Export.Xml
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;
#if NET45_OR_GREATER || NET6_0_OR_GREATER
    using System.Threading.Tasks;
#endif

    internal sealed class XmlCrashDumper : ICrashDataDumpFile
    {
        internal const string DefaultFileName = "CrashDump.xml";
        private const string RootName = "DiagnosticDump";

        private bool m_OwnsStream;
        private Stream m_Stream;
        private XmlWriter m_Writer;

        public void CreateFile(string fileName)
        {
            fileName = CheckCreateFile(fileName);
            try {
                m_Writer = CreateFileInternal(fileName);
                m_Writer.WriteStartElement(RootName);
                m_IsFlushed = false;
            } catch (Exception ex) {
                Log.CrashLog.TraceEvent(System.Diagnostics.TraceEventType.Error, "Error creating crash file: {0}", ex.ToString());
                Close();
                throw;
            }
        }

        public void CreateFile(Stream stream, string path)
        {
            CheckCreateFile(stream, path);
            try {
                m_Writer = CreateFileInternal(stream, path, null);
                m_Writer.WriteStartElement(RootName);
                m_IsFlushed = false;
            } catch (Exception ex) {
                Log.CrashLog.TraceEvent(System.Diagnostics.TraceEventType.Error, "Error creating crash stream: {0}", ex.ToString());
                Close();
                throw;
            }
        }

        private string CheckCreateFile(string fileName)
        {
            ThrowHelper.ThrowIfNull(fileName);
            if (m_Writer is not null) throw new InvalidOperationException("File is already created, cannot create twice");

            if (Directory.Exists(fileName)) {
                Path = fileName;
                fileName = System.IO.Path.Combine(fileName, DefaultFileName);
            } else {
                Path = System.IO.Path.GetDirectoryName(fileName);
            }
            return fileName;
        }

        private void CheckCreateFile(Stream stream, string path)
        {
            ThrowHelper.ThrowIfNull(stream);
            ThrowHelper.ThrowIfNull(path);
            if (m_Writer is not null) throw new InvalidOperationException("File is already created, cannot create twice");

            m_Stream = stream;
            Path = path;
        }

        private XmlWriter CreateFileInternal(string fileName)
        {
            string directory = System.IO.Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            string styleSheetName = string.Format("{0}.xsl", System.IO.Path.GetFileNameWithoutExtension(fileName));
            string styleSheetPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(fileName), styleSheetName);
            CopyTransform(styleSheetPath);

            m_OwnsStream = true;
            m_Stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            return CreateFileInternal(m_Stream, directory, styleSheetName);
        }

        private static XmlWriter CreateFileInternal(Stream stream, string dirPath, string styleSheet)
        {
            if (!Directory.Exists(dirPath)) {
                string message = string.Format("Directory '{0}' not found", dirPath);
                throw new DirectoryNotFoundException(message);
            }

            XmlWriter xmlWriter = XmlWriter.Create(stream, SaveXmlSettings());
            if (styleSheet is not null) {
                string stylesheetInstruction = string.Format("type=\"text/xsl\" href=\"{0}\"", styleSheet);
                xmlWriter.WriteProcessingInstruction("xml-stylesheet", stylesheetInstruction);
            }
            return xmlWriter;
        }

        private static XmlWriterSettings SaveXmlSettings()
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

        public string Path { get; private set; }

        public IDumpTable DumpTable(string tableName, string rowName)
        {
            ThrowHelper.ThrowIfNullOrWhiteSpace(tableName);
            ThrowHelper.ThrowIfNullOrWhiteSpace(rowName);

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

        private static Stream GetStyleSheetResource()
        {
            string resource = CrashReporter.Config.XmlCrashDumper.StyleSheetName;
            if (string.IsNullOrWhiteSpace(resource)) return GetDefaultStyleSheetResource();

            System.Reflection.Assembly assembly;
            string resourceName;
            int resourceClassPos = resource.LastIndexOf(',');

            if (resourceClassPos == -1) {
                // Assume that the user wants the executable (entry)
                assembly = System.Reflection.Assembly.GetEntryAssembly();
                resourceName = resource.Trim();
            } else {
#if NETFRAMEWORK
                string assemblyName = resource.Substring(0, resourceClassPos).Trim();
                resourceName = resource.Substring(resourceClassPos + 1).Trim();
                assembly = GetAssemblyByName(assemblyName);
                if (assembly is null) return GetDefaultStyleSheetResource();
#else
                string assemblyName = resource.AsSpan(0, resourceClassPos).Trim().ToString();
                resourceName = resource.AsSpan(resourceClassPos + 1).Trim().ToString();
                assembly = GetAssemblyByName(assemblyName);
                if (assembly is null) return GetDefaultStyleSheetResource();
#endif
            }

            Stream stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null) return GetDefaultStyleSheetResource();
            return stream;
        }

        private static System.Reflection.Assembly GetAssemblyByName(string name)
        {
            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                System.Reflection.AssemblyName assemblyName = assembly.GetName();
                if (name.Equals(assemblyName.FullName, StringComparison.OrdinalIgnoreCase)) return assembly;
                if (name.Equals(assemblyName.Name, StringComparison.OrdinalIgnoreCase)) return assembly;
            }
            return null;
        }

        private static Stream GetDefaultStyleSheetResource()
        {
            return typeof(XmlCrashDumper).Assembly.GetManifestResourceStream("RJCP.Diagnostics.Crash.Export.Xml.CrashDump.xsl");
        }

        private static void CopyTransform(string outFileName)
        {
            using (Stream stream = GetStyleSheetResource())
            using (FileStream fileCopyStream = new(outFileName, FileMode.Create, FileAccess.Write, FileShare.None)) {
                stream.CopyTo(fileCopyStream);
            }
        }

#if NET45_OR_GREATER || NET6_0_OR_GREATER
        public async Task CreateFileAsync(string fileName)
        {
            fileName = CheckCreateFile(fileName);
            try {
                m_Writer = await CreateFileInternalAsync(fileName).ConfigureAwait(false);
                await m_Writer.WriteStartElementAsync(null, RootName, null).ConfigureAwait(false);
                m_IsFlushed = false;
            } catch (Exception ex) {
                Log.CrashLog.TraceEvent(System.Diagnostics.TraceEventType.Error, "Error creating async crash file: {0}", ex.ToString());
                Close();
                throw;
            }
        }

        public async Task CreateFileAsync(Stream stream, string path)
        {
            CheckCreateFile(stream, path);
            try {
                m_Writer = await CreateFileInternalAsync(stream, path, null).ConfigureAwait(false);
                await m_Writer.WriteStartElementAsync(null, RootName, null).ConfigureAwait(false);
                m_IsFlushed = false;
            } catch (Exception ex) {
                Log.CrashLog.TraceEvent(System.Diagnostics.TraceEventType.Error, "Error creating async crash stream: {0}", ex.ToString());
                Close();
                throw;
            }
        }

        private async Task<XmlWriter> CreateFileInternalAsync(string fileName)
        {
            string directory = System.IO.Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            string styleSheetName = string.Format("{0}.xsl", System.IO.Path.GetFileNameWithoutExtension(fileName));
            string styleSheetPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(fileName), styleSheetName);
            await CopyTransformAsync(styleSheetPath).ConfigureAwait(false);

            m_OwnsStream = true;
            m_Stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            return await CreateFileInternalAsync(m_Stream, directory, styleSheetName).ConfigureAwait(false);
        }

        private static async Task<XmlWriter> CreateFileInternalAsync(Stream stream, string dirPath, string styleSheet)
        {
            if (!Directory.Exists(dirPath)) {
                string message = string.Format("Directory '{0}' not found", dirPath);
                throw new DirectoryNotFoundException(message);
            }

            XmlWriterSettings settings = SaveXmlSettings();
            settings.Async = true;

            XmlWriter xmlWriter = XmlWriter.Create(stream, settings);
            if (styleSheet is not null) {
                string stylesheetInstruction = string.Format("type=\"text/xsl\" href=\"{0}\"", styleSheet);
                await xmlWriter.WriteProcessingInstructionAsync("xml-stylesheet", stylesheetInstruction).ConfigureAwait(false);
            }
            return xmlWriter;
        }

        private static async Task CopyTransformAsync(string outFileName)
        {
            using (Stream stream = GetStyleSheetResource())
            using (FileStream fileCopyStream = new(outFileName, FileMode.Create, FileAccess.Write, FileShare.None)) {
                await stream.CopyToAsync(fileCopyStream).ConfigureAwait(false);
            }
        }

        public Task<IDumpTable> DumpTableAsync(string tableName, string rowName)
        {
            ThrowHelper.ThrowIfNullOrWhiteSpace(tableName);
            ThrowHelper.ThrowIfNullOrWhiteSpace(rowName);
            return DumpTableInternalAsync(tableName, rowName);
        }

        private async Task<IDumpTable> DumpTableInternalAsync(string tableName, string rowName)
        {
            await m_Writer.WriteStartElementAsync(null, tableName, null).ConfigureAwait(false);

            // Update the variables after writing, so when we flush, we close that element. If writing the element would
            // raise an exception, we wouldn't close something that probably wasn't written.
            return new XmlDumpTable(rowName, m_Writer);
        }

        public async Task FlushAsync()
        {
            if (!m_IsFlushed) {
                await m_Writer.WriteEndElementAsync().ConfigureAwait(false);
                await m_Writer.FlushAsync().ConfigureAwait(false);
                await m_Stream.FlushAsync().ConfigureAwait(false);
                m_IsFlushed = true;
            }
        }
#endif

        private void Close()
        {
            if (!m_IsFlushed) Flush();

            if (m_Writer is not null) ((IDisposable)m_Writer).Dispose();
            m_Writer = null;

            if (m_OwnsStream && m_Stream is not null) m_Stream.Dispose();
            m_Stream = null;
        }

        public void Dispose()
        {
            try {
                Close();
            } catch (Exception ex) {
                Log.CrashLog.TraceEvent(System.Diagnostics.TraceEventType.Warning,
                    "XmlCrashDumper Disposing: Ignore exception {0}", ex.ToString());
                /* Ignore all errors when disposing, we might be disposing because of an error */
            }
        }
    }
}
