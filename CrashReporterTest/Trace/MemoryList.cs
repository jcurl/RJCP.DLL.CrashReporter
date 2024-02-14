namespace RJCP.Diagnostics.Trace
{
    using System.Collections;
    using System.Collections.Generic;

    public class MemoryList : IMemoryLog
    {
        private readonly List<LogEntry> m_List = new();

        public int Count { get { return m_List.Count; } }

        public bool IsReadOnly { get { return false; } }

        public void Add(LogEntry item)
        {
            m_List.Add(item);
        }

        public void Clear()
        {
            m_List.Clear();
        }

        public bool Contains(LogEntry item)
        {
            return m_List.Contains(item);
        }

        public void CopyTo(LogEntry[] array, int arrayIndex)
        {
            m_List.CopyTo(array, arrayIndex);
        }

        public bool Remove(LogEntry item)
        {
            return m_List.Remove(item);
        }

        public IEnumerator<LogEntry> GetEnumerator()
        {
            return m_List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
