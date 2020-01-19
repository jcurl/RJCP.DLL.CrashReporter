namespace RJCP.Diagnostics.Watchdog.Timer
{
    /// <summary>
    /// Interface for retrieving the clock value from a timer source.
    /// </summary>
    public interface ITimerSource
    {
        /// <summary>
        /// Gets the clock.
        /// </summary>
        /// <returns>A 32-bit signed integer whose value represents the current time in milliseconds.</returns>
        int GetClock();
    }
}
