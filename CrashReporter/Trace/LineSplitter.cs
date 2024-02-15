namespace RJCP.Diagnostics.Trace
{
    using System.Collections.Generic;
    using System.Text;

    internal class LineSplitter
    {
        private readonly StringBuilder m_Line = new();
        private readonly List<string> m_Lines = new();

        public IEnumerable<string> Append(string line)
        {
            AppendInternal(line);
            return GetListAndClear();
        }

        public IEnumerable<string> AppendLine(string line)
        {
            AppendInternal(line);
            m_Lines.Add(m_Line.ToString());
            m_Line.Clear();
            return GetListAndClear();
        }

        public bool IsCached { get { return m_Line.Length > 0; } }

        private IEnumerable<string> GetListAndClear()
        {
            string[] result = m_Lines.ToArray();
            m_Lines.Clear();
            return result;
        }

        private void AppendInternal(string line)
        {
            if (string.IsNullOrEmpty(line)) return;

            string[] lines = line.Split('\n');
            m_Line.Append(lines[0]);
            if (lines.Length == 1) {
                // There was no newline
                return;
            }

            m_Lines.Add(m_Line.ToString());
            m_Line.Clear();
            if (lines.Length > 2) {
                for (int i = 1; i < lines.Length - 1; i++) {
                    m_Lines.Add(lines[i]);
                }
            }
#if NETFRAMEWORK
            m_Line.Append(lines[lines.Length - 1]);
#else
            m_Line.Append(lines[^1]);
#endif
        }
    }
}
