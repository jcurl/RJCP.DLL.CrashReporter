namespace RJCP.Diagnostics.Watchdog.Timer
{
    using System;

    /// <summary>
    /// Provides an interface for setting up single shot timers.
    /// </summary>
    public interface ISingleShotTimer
    {
        /// <summary>
        /// Occurs when a timeout occurs from a previous call to <see cref="SetDelay(int)"/>.
        /// </summary>
        event EventHandler AlarmEvent;

        /// <summary>
        /// Sets the timeout for the next <see cref="AlarmEvent"/>.
        /// </summary>
        /// <param name="timeout">The delay in milliseconds for the next timeout event.</param>
        void SetDelay(int timeout);
    }
}
