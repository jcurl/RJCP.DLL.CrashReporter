namespace RJCP.Diagnostics.CrashExport
{
    using System;
    using System.Collections.Generic;
    using CodeQuality.NUnitExtensions;
    using MemoryDump;
#if NET45_OR_GREATER
    using System.Threading.Tasks;
#endif

    public sealed class MemoryCrashDataDumpFile : ICrashDataDumpFile
    {
        private readonly Dictionary<string, List<MemoryCrashDumpTable>> m_Blocks = new Dictionary<string, List<MemoryCrashDumpTable>>();

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

        private ScratchPad m_Scratch;

        public string Path
        {
            get
            {
                if (m_Scratch == null) {
                    m_Scratch = new ScratchPad(ScratchOptions.CreateScratch | ScratchOptions.KeepCurrentDir);
                }
                return m_Scratch.Path;
            }
        }

        public IDumpTable DumpTable(string tableName, string rowName)
        {
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));
            if (rowName == null) throw new ArgumentNullException(nameof(rowName));
            if (IsDisposed) throw new ObjectDisposedException(nameof(MemoryCrashDumpTable));
            if (m_IsFlushed) throw new InvalidOperationException("Object flushed, writing is not allowed");

            return DumpTableInternal(tableName, rowName);
        }

        private IDumpTable DumpTableInternal(string tableName, string rowName)
        {
            MemoryCrashDumpTable block = new MemoryCrashDumpTable(tableName, rowName);
            lock (m_Blocks) {
                if (IsSynchronous && CheckIncomplete(out string _))
                    throw new InvalidOperationException("DumpBlock requested while another DumpBlock is active in synchronous mode");
                if (!m_Blocks.TryGetValue(tableName, out List<MemoryCrashDumpTable> blockList)) {
                    blockList = new List<MemoryCrashDumpTable>();
                    m_Blocks.Add(tableName, blockList);
                }
                blockList.Add(block);
            }
            return block;
        }

        private bool CheckIncomplete(out string name)
        {
            foreach (List<MemoryCrashDumpTable> blockList in m_Blocks.Values) {
                for (int i = 0; i < blockList.Count; i++) {
                    MemoryCrashDumpTable knownBlock = blockList[i];
                    if (!knownBlock.IsDisposed) {
                        name = string.Format("{0}[{1}]", knownBlock.TableName, i);
                        return true;
                    }
                }
            }
            name = string.Empty;
            return false;
        }

        bool m_IsFlushed;

        public void Flush()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(MemoryCrashDumpTable));
            if (m_IsFlushed) throw new InvalidOperationException("Flushed twice, useless operation");

            /* Nothing to flush, as this is memory only */
            m_IsFlushed = true;
        }

#if NET45_OR_GREATER
        public Task<IDumpTable> DumpTableAsync(string tableName, string rowName)
        {
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));
            if (rowName == null) throw new ArgumentNullException(nameof(rowName));
            if (IsDisposed) throw new ObjectDisposedException(nameof(MemoryCrashDumpTable));
            if (m_IsFlushed) throw new InvalidOperationException("Object flushed, writing is not allowed");

            return Task.Run(() => { return DumpTableInternal(tableName, rowName); });
        }

        private readonly static Task Completed = Task.FromResult(true);    // .NET 4.6 and later has Task>Completed

        public Task FlushAsync()
        {
            Flush();
            return Completed;
        }
#endif

        private class TableEntry : ITables
        {
            public TableEntry(IList<MemoryCrashDumpTable> tables) { Table = tables; }

            public IList<MemoryCrashDumpTable> Table { get; private set; }
        }

        public ITables this[string table]
        {
            get
            {
                lock (m_Blocks) {
                    if (m_Blocks.ContainsKey(table)) return new TableEntry(m_Blocks[table]);
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

        public void DumpContent()
        {
            foreach (IList<MemoryCrashDumpTable> tableList in m_Blocks.Values) {
                foreach (MemoryCrashDumpTable table in tableList) {
                    Console.WriteLine("Table: {0}", table.TableName);
                    foreach (IFields row in table) {
                        Console.Write("  {0}:", table.RowName);
                        bool first = true;
                        foreach (string field in table.Headers) {
                            if (first) {
                                Console.Write(" {0}={1}", field, row.Field[field]);
                                first = false;
                            } else {
                                Console.Write(", {0}={1}", field, row.Field[field]);
                            }
                        }
                        Console.WriteLine(string.Empty);
                    }
                }
            }
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (m_Scratch != null) m_Scratch.Dispose();

            if (!m_IsFlushed) {
                Console.WriteLine("Dispose called on file without flushing. Recommend flushing. Not critical, but can lead to synchronous flush");
            }

            bool properlyDisposed = true;
            lock (m_Blocks) {
                if (CheckIncomplete(out string name)) {
                    Console.WriteLine("Dispose called with table '{0}' not disposed.", name);
                    properlyDisposed = false;
                }
            }
            if (!properlyDisposed) throw new InvalidOperationException("DumpTable not disposed prior");

            IsDisposed = true;
        }
    }
}
