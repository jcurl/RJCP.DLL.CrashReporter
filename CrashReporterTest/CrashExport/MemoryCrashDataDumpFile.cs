namespace RJCP.Diagnostics.CrashExport
{
    using System;
    using System.Collections.Generic;

    public sealed class MemoryCrashDataDumpFile : ICrashDataDumpFile
    {
        private Dictionary<string, MemoryCrashDumpTable> m_Blocks = new Dictionary<string, MemoryCrashDumpTable>();

        /// <summary>
        /// Gets a value indicating if this instance can support writing blocks asynchronously.
        /// </summary>
        /// <value>
        /// Returns <see langword="true"/> if this instance is synchronous; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// If this object is synchronous (the value of this property is <see langword="true"/>), obtaining and using
        /// the <see cref="IDumpBlock"/> returned by <see cref="ICrashDataDumpFile.DumpBlock(String)"/> must by
        /// synchronous (the previous block must be disposed prior to obtaining a new one). For example, if the
        /// underlying implementing writes directly to disk, this would be typically a synchronous implementation.
        /// <para>This property can be written to for the purposes of testing.</para>
        /// </remarks>
        public bool IsSynchronous { get; set; } = true;

        private NUnit.Framework.ScratchPad m_Scratch;

        public string Path
        {
            get
            {
                if (m_Scratch == null) {
                    m_Scratch = new NUnit.Framework.ScratchPad(NUnit.Framework.ScratchOptions.CreateScratch | NUnit.Framework.ScratchOptions.KeepCurrentDir);
                }
                return m_Scratch.Path;
            }
        }

        public IDumpTable DumpTable(string tableName, string rowName)
        {
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));
            if (rowName == null) throw new ArgumentNullException(nameof(rowName));
            if (IsDisposed) throw new ObjectDisposedException(nameof(MemoryCrashDumpTable));
            lock (m_Blocks) {
                if (m_Blocks.ContainsKey(tableName)) throw new ArgumentException("Duplicate element", nameof(tableName));
            }
            if (m_IsFlushed) throw new InvalidOperationException("Object flushed, writing is not allowed");

            return DumpTableInternal(tableName, rowName);
        }

        private IDumpTable DumpTableInternal(string tableName, string rowName)
        {
            MemoryCrashDumpTable block = new MemoryCrashDumpTable(tableName, rowName);
            lock (m_Blocks) {
                if (IsSynchronous) {
                    foreach (var knownBlock in m_Blocks) {
                        if (!knownBlock.Value.IsDisposed)
                            throw new InvalidOperationException("DumpBlock requested while another DumpBlock is active in synchronous mode");
                    }
                }
                m_Blocks.Add(tableName, block);
            }
            return block;
        }

        bool m_IsFlushed;

        public void Flush()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(MemoryCrashDumpTable));
            if (m_IsFlushed) throw new InvalidOperationException("Flushed twice, useless operation");

            /* Nothing to flush, as this is memory only */
            m_IsFlushed = true;
        }

        public MemoryCrashDumpTable this[string table]
        {
            get
            {
                lock (m_Blocks) {
                    if (m_Blocks.ContainsKey(table)) return m_Blocks[table];
                }
                string message = string.Format("Table '{0}' not found", table);
                throw new KeyNotFoundException(message);
            }
        }

        public int Count
        {
            get
            {
                lock (m_Blocks) {
                    return m_Blocks.Count;
                }
            }
        }

        public bool IsDisposed { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Blocker Code Smell", "S3877:Exceptions should not be thrown from unexpected methods",
            Justification = "Testcase specific code, only to ensure implementations are correct.")]
        public void Dispose()
        {
            if (m_Scratch != null) m_Scratch.Dispose();

            if (!m_IsFlushed) {
                Console.WriteLine("Dispose called on file without flushing. Recommend flushing. Not critical, but can lead to synchronous flush");
            }

            bool properlyDisposed = true;
            lock (m_Blocks) {
                foreach (var block in m_Blocks) {
                    if (!block.Value.IsDisposed) {
                        Console.WriteLine("Dispose called with table '{0}' not disposed.", block.Key);
                        properlyDisposed = false;
                    }
                }
            }
            if (!properlyDisposed) throw new InvalidOperationException("DumpTable not disposed prior");

            IsDisposed = true;
        }
    }
}
