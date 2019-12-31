namespace RJCP.Diagnostics.Trace
{
    using System;

    internal class InternalClock
    {
        private int m_Epoch;
        private uint m_Zero;
        private uint m_Last;

        public static readonly InternalClock Instance = new InternalClock();

        public long GetClock()
        {
            uint current = unchecked((uint)Environment.TickCount);
            if (current < m_Last) {
                m_Epoch++;
            }
            m_Last = current;
            return (m_Epoch << 32) + m_Last - m_Zero;
        }

        public void SetZero()
        {
            m_Epoch = 0;
            m_Zero = unchecked((uint)Environment.TickCount);
            m_Last = m_Zero;
        }
    }
}
