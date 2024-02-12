namespace RJCP.Diagnostics.Trace
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// A simple prioritized logging mechanism
    /// </summary>
    /// <remarks>
    /// This object is not thread safe.
    /// </remarks>
    public class SimplePrioMemoryLog : IMemoryLog
    {
        private const int CriticalDefault = 200;
        private const int ErrorDefault = 200;
        private const int WarningDefault = 300;
        private const int InfoDefault = 400;
        private const int VerboseDefault = 500;
        private const int OtherDefault = 200;
        private const int MaxDefault = CriticalDefault + ErrorDefault + WarningDefault + InfoDefault + VerboseDefault + OtherDefault + 200;

        private sealed class Log
        {
            public Log(int defaultMinimum)
            {
                Minimum = defaultMinimum;
            }

            private readonly Queue<LogEntry> m_Log = new Queue<LogEntry>();

            public Queue<LogEntry> LogList { get { return m_Log; } }

            private int m_Minimum;

            public int Minimum
            {
                get { return m_Minimum; }
                set
                {

                    if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Buffer count is negative");
                    if (value == 0) value = 1;
                    m_Minimum = value;
                }
            }
        }

        // The logs. m_Other is the lowest priority for all undefined log levels.
        private readonly Log m_Critical = new Log(CriticalDefault);
        private readonly Log m_Error = new Log(ErrorDefault);
        private readonly Log m_Warning = new Log(WarningDefault);
        private readonly Log m_Info = new Log(InfoDefault);
        private readonly Log m_Verbose = new Log(VerboseDefault);
        private readonly Log m_Other = new Log(OtherDefault);

        // Defines the prioritized order of the logs. First is lowest priority.
        private readonly List<KeyValuePair<TraceEventType, Log>> m_Log;

        // Maps quickly to a log list for adding logs.
        private readonly Dictionary<TraceEventType, Log> m_LogMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimplePrioMemoryLog"/> class.
        /// </summary>
        public SimplePrioMemoryLog()
        {
            m_Log = new List<KeyValuePair<TraceEventType, Log>>() {
                new KeyValuePair<TraceEventType, Log>(TraceEventType.Verbose, m_Verbose),
                new KeyValuePair<TraceEventType, Log>(TraceEventType.Information, m_Info),
                new KeyValuePair<TraceEventType, Log>(TraceEventType.Warning, m_Warning),
                new KeyValuePair<TraceEventType, Log>(TraceEventType.Error, m_Error),
                new KeyValuePair<TraceEventType, Log>(TraceEventType.Critical, m_Critical),
            };
            m_LogMap = new Dictionary<TraceEventType, Log>() {
                { TraceEventType.Verbose, m_Verbose },
                { TraceEventType.Information, m_Info },
                { TraceEventType.Warning, m_Warning },
                { TraceEventType.Error, m_Error },
                { TraceEventType.Critical, m_Critical },
            };
        }

        /// <summary>
        /// Gets or sets the minimum number of allowed critical log entries in the memory buffer.
        /// </summary>
        /// <value>
        /// The minimum number of allowed critical log entries in the memory buffer.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">Buffer count is negative.</exception>
        public int Critical
        {
            get { return m_Critical.Minimum; }
            set
            {
                m_Critical.Minimum = value;
                UpdateMaxValue();
            }
        }

        /// <summary>
        /// Gets or sets the minimum number of allowed error log entries in the memory buffer.
        /// </summary>
        /// <value>
        /// The minimum number of allowed error log entries in the memory buffer.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">Buffer count is negative.</exception>
        public int Error
        {
            get { return m_Error.Minimum; }
            set
            {
                m_Error.Minimum = value;
                UpdateMaxValue();
            }
        }

        /// <summary>
        /// Gets or sets the minimum number of allowed warning log entries in the memory buffer.
        /// </summary>
        /// <value>
        /// The minimum number of allowed warning log entries in the memory buffer.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">Buffer count is negative.</exception>
        public int Warning
        {
            get { return m_Warning.Minimum; }
            set
            {
                m_Warning.Minimum = value;
                UpdateMaxValue();
            }
        }

        /// <summary>
        /// Gets or sets the minimum number of allowed informational log entries in the memory buffer.
        /// </summary>
        /// <value>
        /// The minimum number of allowed informational log entries in the memory buffer.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">Buffer count is negative.</exception>
        public int Info
        {
            get { return m_Info.Minimum; }
            set
            {
                m_Info.Minimum = value;
                UpdateMaxValue();
            }
        }

        /// <summary>
        /// Gets or sets the minimum number of allowed verbose log entries in the memory buffer.
        /// </summary>
        /// <value>
        /// The minimum number of allowed verbose log entries in the memory buffer.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">Buffer count is negative.</exception>
        public int Verbose
        {
            get { return m_Verbose.Minimum; }
            set
            {
                m_Verbose.Minimum = value;
                UpdateMaxValue();
            }
        }

        /// <summary>
        /// Gets or sets the minimum number of allowed other log entries in the memory buffer.
        /// </summary>
        /// <value>
        /// The minimum number of allowed other log entries in the memory buffer.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">Buffer count is negative.</exception>
        public int Other
        {
            get { return m_Other.Minimum; }
            set
            {
                m_Other.Minimum = value;
                UpdateMaxValue();
            }
        }

        private int m_Max = MaxDefault;

        /// <summary>
        /// Gets or sets the maximum number of allowed log entries in the memory buffer.
        /// </summary>
        /// <value>
        /// The maximum number of allowed log entries in the memory buffer.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">Buffer count is negative.</exception>
        public int Total
        {
            get { return m_Max; }
            set
            {
                m_Max = value;
                UpdateMaxValue();
            }
        }

        private void UpdateMaxValue()
        {
            m_Max = Math.Max(m_Max, m_Critical.Minimum + m_Error.Minimum + m_Warning.Minimum + m_Info.Minimum + m_Verbose.Minimum + m_Other.Minimum);
        }

        /// <summary>
        /// Gets the total number of messages in the log.
        /// </summary>
        /// <value>
        /// The total number of messages in the count.
        /// </value>
        public int Count { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if this instance is read only; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// This list is read/write and so always returns <see langword="true"/>;
        /// </remarks>
        public bool IsReadOnly { get { return false; } }

        /// <summary>
        /// Adds the specified entry.
        /// </summary>
        /// <param name="item">The entry.</param>
        public void Add(LogEntry item)
        {
            if (Count >= m_Max) {
                // Remove the lowest priority entry first and add this to the correct list.
                foreach (var log in m_Log) {
                    int equalLevel = (log.Value.Minimum > 0 && item.EventType == log.Key) ? 1 : 0;
                    if (log.Value.LogList.Count > log.Value.Minimum - equalLevel) {
                        log.Value.LogList.Dequeue();
                        Enqueue(item);
                        return;
                    }
                }
            }
            Enqueue(item);
            Count++;
        }

        private void Enqueue(LogEntry entry)
        {
            if (m_LogMap.TryGetValue(entry.EventType, out Log list)) {
                list.LogList.Enqueue(entry);
            } else {
                m_Other.LogList.Enqueue(entry);
            }
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            foreach (var log in m_Log) {
                log.Value.LogList.Clear();
            }
            m_Other.LogList.Clear();
        }

        /// <summary>
        /// Determines whether this instance contains the log entry.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <returns>
        /// <see langword="true"/> if this collection contains the specified item, else <see langword="false"/>.
        /// </returns>
        public bool Contains(LogEntry item)
        {
            foreach (var log in m_Log) {
                if (log.Value.LogList.Contains(item)) return true;
            }
            return m_Other.LogList.Contains(item);
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">Index in the array to start copying to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex"/> is less than zero;
        /// <para>- or -</para>
        /// <paramref name="arrayIndex"/> offset and collection length would exceed boundaries of
        /// <paramref name="array"/>.
        /// </exception>
        public void CopyTo(LogEntry[] array, int arrayIndex)
        {
            ThrowHelper.ThrowIfNull(array);
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Array Index is less than 0");

            int requiredLength = 0;
            foreach (var log in m_Log) {
                requiredLength += m_Log.Count;
            }
            if (arrayIndex > requiredLength - array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Array index offset and collection length would exceed array boundaries");

            int offset = arrayIndex;
            foreach (var log in m_Log) {
                log.Value.LogList.CopyTo(array, offset);
                offset += log.Value.LogList.Count;
            }
            m_Other.LogList.CopyTo(array, offset);
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// <see langword="true"/> if the item was found or removed, <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">Cannot remove items from the collection.</exception>
        /// <remarks>This collection doesn't support removing items.</remarks>
        public bool Remove(LogEntry item)
        {
            throw new InvalidOperationException("Cannot remove items from the collection");
        }

        #region Enumeration
        private sealed class MemoryLogEnumerator : IEnumerator<LogEntry>
        {
            private sealed class Indexer
            {
                private readonly Log m_List;
                private IEnumerator<LogEntry> m_Enumerator;

                public Indexer(Log list)
                {
                    m_List = list;
                    Reset();
                }

                public bool IsValid { get; private set; }

                public LogEntry Current { get; set; }

                public void MoveNext()
                {
                    if (!IsValid) return;
                    bool next = m_Enumerator.MoveNext();
                    if (next) Current = m_Enumerator.Current;
                    IsValid = next;
                }

                public void Reset()
                {
                    if (m_Enumerator != null) m_Enumerator.Dispose();
                    m_Enumerator = m_List.LogList.GetEnumerator();
                    IsValid = true;
                    MoveNext();
                }

                public void Close()
                {
                    if (m_Enumerator != null) m_Enumerator.Dispose();
                    Current = null;
                    m_Enumerator = null;
                    IsValid = false;
                }
            }

            private readonly List<Indexer> m_Logs;

            public MemoryLogEnumerator(SimplePrioMemoryLog parent)
            {
                m_Logs = new List<Indexer>() {
                    new Indexer(parent.m_Critical),
                    new Indexer(parent.m_Error),
                    new Indexer(parent.m_Warning),
                    new Indexer(parent.m_Info),
                    new Indexer(parent.m_Verbose),
                    new Indexer(parent.m_Other),
                };
            }

            private LogEntry m_Current;

            public LogEntry Current { get { return m_Current; } }

            object IEnumerator.Current { get { return m_Current; } }

            public bool MoveNext()
            {
                Indexer next = null;

                foreach (var log in m_Logs) {
                    if (log.IsValid) {
                        if (next == null) {
                            next = log;
                        } else if (log.Current.DateTime < next.Current.DateTime) {
                            next = log;
                        }
                    }
                }
                if (next == null) return false;
                m_Current = next.Current;
                next.MoveNext();
                return true;
            }

            public void Reset()
            {
                foreach (var log in m_Logs) {
                    log.Reset();
                }
            }

            public void Dispose()
            {
                foreach (var log in m_Logs) {
                    log.Close();
                }
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>An enumerator which sorts items in this list by the clock.</returns>
        /// <remarks>
        /// Enumerating this collection won't necessarily result in items being strictly sorted by the clock if the items are not added
        /// sorted.
        /// </remarks>
        public IEnumerator<LogEntry> GetEnumerator()
        {
            return new MemoryLogEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
