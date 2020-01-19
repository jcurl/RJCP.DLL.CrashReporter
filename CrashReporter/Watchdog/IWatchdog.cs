namespace RJCP.Diagnostics.Watchdog
{
    using System;

    /// <summary>
    /// Watchdog interface.
    /// </summary>
    public interface IWatchdog
    {
        /// <summary>
        /// Registers the watchdog of the specified name.
        /// </summary>
        /// <param name="name">The name of the watchdog which is case sensitive.</param>
        /// <param name="warning">The timeout before a warning is generated.</param>
        /// <param name="critical">The timeout before a watchdog timeout occurs and the program ends.</param>
        /// <returns>
        /// Returns <see langword="true"/> if the watchdog was registered, <see langword="false"/> if the watchdog was
        /// already registered.
        /// </returns>
        bool Register(string name, int warning, int critical);

        /// <summary>
        /// Unregisters the watchdog of the specified name.
        /// </summary>
        /// <param name="name">The name of the watchdog to unregister.</param>
        /// <returns>
        /// Returns <see langword="true"/> if the watchdog was registered and is now deregistered,
        /// <see langword="false"/> otherwise as the watchdog was not previously registered.
        /// </returns>
        bool Unregister(string name);

        /// <summary>
        /// Feed the watchdog of the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// Returns <see langword="true"/> if the ping reset a watchdog item, <see langword="false"/> if the watchdog
        /// item didn't exist.
        /// </returns>
        bool Ping(string name);

        /// <summary>
        /// Occurs when a watchdog warning has occurred.
        /// </summary>
        event EventHandler<WatchdogEventArgs> WarningEvent;

        /// <summary>
        /// Occurs when a watchdog timeout has occurred.
        /// </summary>
        event EventHandler<WatchdogEventArgs> CriticalEvent;
    }
}
