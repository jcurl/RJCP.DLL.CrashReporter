namespace RJCP.Diagnostics.Watchdog.Timer
{
    using System;

    /// <summary>
    /// Provides a millisecond timer source based on Windows clock tick.
    /// </summary>
    public class MonotonicTimerSource : ITimerSource
    {
        /// <summary>
        /// Gets the clock.
        /// </summary>
        /// <returns>A 32-bit signed integer whose value represents the current time in milliseconds.</returns>
        public int GetClock()
        {
            return Environment.TickCount;
        }
    }
}
