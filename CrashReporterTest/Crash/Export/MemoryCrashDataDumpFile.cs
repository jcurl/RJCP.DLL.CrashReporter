namespace RJCP.Diagnostics.Crash.Export
{
    using System;
    using System.Collections.Generic;
    using CodeQuality.NUnitExtensions;
    using MemoryDump;
#if !NET40_LEGACY
    using System.Threading.Tasks;
#endif

    public sealed class MemoryCrashDataDumpFile : ICrashDataDumpFile
    {
        private readonly Dictionary<string, List<MemoryCrashDumpTable>> m_Blocks = new();
        private ScratchPad m_Scratch;

        public string Path
        {
            get
            {
                m_Scratch ??= new ScratchPad(ScratchOptions.CreateScratch | ScratchOptions.KeepCurrentDir);
                return m_Scratch.Path;
            }
        }

        public IDumpTable DumpTable(string tableName, string rowName)
        {
            ThrowHelper.ThrowIfNull(tableName);
            ThrowHelper.ThrowIfNull(rowName);
            ThrowHelper.ThrowIfDisposed(IsDisposed, this);
            if (m_IsFlushed) throw new InvalidOperationException("Object flushed, writing is not allowed");

            return DumpTableInternal(tableName, rowName);
        }

        private IDumpTable DumpTableInternal(string tableName, string rowName)
        {
            MemoryCrashDumpTable block = new(tableName, rowName);
            lock (m_Blocks) {
                // Ensures that all tables in the file are already disposed. This means that the `CrashData` is dumping
                // the `ICrashDataExport` objects serially (it can collect them if it wants in parallel, but dumping
                // must still be done serially).
                if (CheckIncomplete(out string _))
                    throw new InvalidOperationException("DumpBlock requested while another DumpBlock is active");

                // The user may dump multiple tables of the same name.
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
                        name = $"{knownBlock.TableName}[{i}]";
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
            ThrowHelper.ThrowIfDisposed(IsDisposed, this);
            if (m_IsFlushed) throw new InvalidOperationException("Flushed twice, useless operation");

            /* Nothing to flush, as this is memory only */
            m_IsFlushed = true;
        }

#if !NET40_LEGACY
        public Task<IDumpTable> DumpTableAsync(string tableName, string rowName)
        {
            ThrowHelper.ThrowIfNull(tableName);
            ThrowHelper.ThrowIfNull(rowName);
            ThrowHelper.ThrowIfDisposed(IsDisposed, this);
            if (m_IsFlushed) throw new InvalidOperationException("Object flushed, writing is not allowed");

            // Start a task that it is started immediately on the thread-pool. This promotes testing if multiple blocks
            // are being dumped simultaneously, that will result in an exception in the task.
            return Task.Run(() => { return DumpTableInternal(tableName, rowName); });
        }

        public Task FlushAsync()
        {
            Flush();
            return Task.CompletedTask;
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
                    if (m_Blocks.TryGetValue(table, out var tableEntry))
                        return new TableEntry(tableEntry);
                }
                throw new KeyNotFoundException($"Table '{table}' not found");
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
                    Console.WriteLine($"Table: {table.TableName}");
                    foreach (IFields row in table) {
                        Console.Write($"  {table.RowName}:");
                        bool first = true;
                        foreach (string field in table.Headers) {
                            if (first) {
                                Console.Write($" {field}={row.Field[field]}");
                                first = false;
                            } else {
                                Console.Write($", {field}={row.Field[field]}");
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
            if (m_Scratch is not null) m_Scratch.Dispose();

            if (!m_IsFlushed) {
                Console.WriteLine("Dispose called on file without flushing. Recommend flushing. Not critical, but can lead to synchronous flush");
            }

            bool properlyDisposed = true;
            lock (m_Blocks) {
                if (CheckIncomplete(out string name)) {
                    Console.WriteLine($"Dispose called with table '{name}' not disposed.");
                    properlyDisposed = false;
                }
            }
            if (!properlyDisposed) throw new InvalidOperationException("DumpTable not disposed prior");

            IsDisposed = true;
        }
    }
}
