namespace RJCP.Diagnostics.Watchdog
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Event arguments for a watchdog timeout event.
    /// </summary>
    public class WatchdogEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WatchdogEventArgs"/> class.
        /// </summary>
        /// <param name="expired">The enumerable list of expired events.</param>
        public WatchdogEventArgs(IEnumerable<string> expired)
        {
            Expired = expired;
        }

        /// <summary>
        /// Gets the enumerable list of expired events.
        /// </summary>
        /// <value>The enumerable list of expired events.</value>
        public IEnumerable<string> Expired { get; private set; }
    }
}
