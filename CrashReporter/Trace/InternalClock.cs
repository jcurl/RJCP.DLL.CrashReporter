namespace RJCP.Diagnostics.Trace
{
    using System;

    /// <summary>
    /// A simple millisecond clock which extends the 32-bit to 64-bit.
    /// </summary>
    /// <remarks>
    /// For the extension to 64-bit to work, <see cref="GetClock()"/> must be called at least once every 48 days. The
    /// duration comes from the maximum time, in milliseconds, that a 32-bit number can hold. If rollover is occurred
    /// (the clock goes backwards in time), the upper 32-bit value is incremented.
    /// </remarks>
    internal class InternalClock
    {
        private int m_Epoch;
        private uint m_Zero;
        private uint m_Last;

        public static readonly InternalClock Instance = new();

        /// <summary>
        /// Gets the clock.
        /// </summary>
        /// <returns>A clock value since <see cref="SetZero"/> was called.</returns>
        public long GetClock()
        {
            uint current = unchecked((uint)Environment.TickCount);
            if (current < m_Last) {
                m_Epoch++;
            }
            m_Last = current;
            return (m_Epoch << 32) + m_Last - m_Zero;
        }

        /// <summary>
        /// Resets the clock to zero.
        /// </summary>
        public void SetZero()
        {
            m_Epoch = 0;
            m_Zero = unchecked((uint)Environment.TickCount);
            m_Last = m_Zero;
        }

        private bool m_Init;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <remarks>Sets the clock to zero on the first call. All subsequent calls are ignored.</remarks>
        public void Initialize()
        {
            if (!m_Init) {
                SetZero();
                m_Init = true;
            }
        }
    }
}
