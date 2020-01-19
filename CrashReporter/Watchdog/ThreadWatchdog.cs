namespace RJCP.Diagnostics.Watchdog
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// A Watchdog for threads.
    /// </summary>
    public sealed class ThreadWatchdog : IWatchdog
    {
        private WatchdogList m_Watchdog;
        private Timer.ISingleShotTimer m_Timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadWatchdog"/> class.
        /// </summary>
        public ThreadWatchdog()
        {
            m_Watchdog = new WatchdogList();
            m_Timer = new Timer.SingleShotTimer();
            InitializeTimer();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadWatchdog"/> class with custom timers.
        /// </summary>
        /// <param name="timerSource">The timer source providing the 32-bit clock.</param>
        /// <param name="timer">The timer object that raises events on timer changes.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="timerSource"/> may not be <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="timer"/> may not be <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// Using custom timer objects allow to virtualize timers, or to provide higher resolution timers than the
        /// defaults. Virtualized timers allow for test cases to run independently of the system clock, making test
        /// case faster and more reliable (independent of any system timer jitter or system dependent behavior).
        /// </remarks>
        public ThreadWatchdog(Timer.ITimerSource timerSource, Timer.ISingleShotTimer timer)
        {
            if (timerSource == null) throw new ArgumentNullException(nameof(timerSource));
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            m_Watchdog = new WatchdogList(timerSource);
            m_Timer = timer;
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            m_Timer.AlarmEvent += WatchdogTimeoutEvent;
        }

        private readonly object m_TimerSyncLock = new object();

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
        public bool Register(string name, int warning, int critical)
        {
            lock (m_TimerSyncLock) {
                bool added = m_Watchdog.Add(name, warning, critical);
                if (added) {
                    m_Timer.SetDelay(m_Watchdog.GetNextExpiry());
                    Log.Watchdog.TraceEvent(TraceEventType.Information, 0,
                        "Watchdog Register: '{0}'; warning {1}ms; critical {2}ms", name, warning, critical);
                } else {
                    if (Log.Watchdog.Switch.ShouldTrace(TraceEventType.Warning)) {
                        WatchdogData registerData = m_Watchdog[name];
                        string message = string.Format(
                            "Watchdog Register: '{0}' already registered\n" +
                            " Registered at {1}: Stack:\n{2}" +
                            " Call Stack:\n{3}",
                            name, registerData.RegisterTime.ToString("u"), registerData.RegisterStack?.ToString() ?? "(none)",
                            new StackFrame(true)?.ToString() ?? "(none)");
                        Log.Watchdog.TraceEvent(TraceEventType.Warning, 0, message);
                    }
                }

                return added;
            }
        }

        /// <summary>
        /// Unregisters the watchdog of the specified name.
        /// </summary>
        /// <param name="name">The name of the watchdog to unregister.</param>
        /// <returns>
        /// Returns <see langword="true"/> if the watchdog was registered and is now deregistered,
        /// <see langword="false"/> otherwise as the watchdog was not previously registered.
        /// </returns>
        public bool Unregister(string name)
        {
            lock (m_TimerSyncLock) {
                bool removed = m_Watchdog.Remove(name);
                if (removed) {
                    m_Timer.SetDelay(m_Watchdog.GetNextExpiry());
                    Log.Watchdog.TraceEvent(TraceEventType.Information, 0, "Watchdog Unregister: '{0}'", name);
                } else {
                    if (Log.Watchdog.Switch.ShouldTrace(TraceEventType.Warning)) {
                        string message = string.Format(
                            "Watchdog Unregister: '{0}' already unregistered\n" +
                            " Call Stack:\n{1}",
                            name, new StackFrame(true)?.ToString() ?? "(none)");
                        Log.Watchdog.TraceEvent(TraceEventType.Warning, 0, message);
                    }
                }

                return removed;
            }
        }

        /// <summary>
        /// Feed the watchdog of the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// Returns <see langword="true"/> if the ping reset a watchdog item, <see langword="false"/> if the watchdog
        /// item didn't exist.
        /// </returns>
        public bool Ping(string name)
        {
            lock (m_TimerSyncLock) {
                bool ping = m_Watchdog.Reset(name);
                if (ping) {
                    m_Timer.SetDelay(m_Watchdog.GetNextExpiry());
                    Log.Watchdog.TraceEvent(TraceEventType.Verbose, 0,
                        "Watchdog Ping: '{0}'", name);
                } else {
                    if (Log.Watchdog.Switch.ShouldTrace(TraceEventType.Warning)) {
                        string message = string.Format(
                            "Watchdog Ping: '{0}' not registered\n" +
                            " Call Stack:\n{1}",
                            name, new StackFrame(true)?.ToString() ?? "(none)");
                        Log.Watchdog.TraceEvent(TraceEventType.Warning, 0, message);
                    }
                }
                return ping;
            }
        }

        private void WatchdogTimeoutEvent(object sender, EventArgs e)
        {
            lock (m_TimerSyncLock) {
                bool isWarning = false;
                bool isCritical = false;
                IEnumerable<string> warnings = m_Watchdog.GetWarnings();
                IEnumerable<string> timeouts = m_Watchdog.GetCriticalTimeouts();

                // Log all warnings
                foreach (string warning in warnings) {
                    isWarning = true;
                    if (Log.Watchdog.Switch.ShouldTrace(TraceEventType.Warning)) {
                        WatchdogData warningData = m_Watchdog[warning];
                        string message = string.Format(
                            "Watchdog warning: '{0}' timeout {1}ms\n" +
                            " Registered at {2}: Stack:\n{3}" +
                            " Last ping at {4}: Stack:\n{5}" +
                            " Thread {6}",
                            warning, warningData.WarningTimeout,
                            warningData.RegisterTime.ToString("u"), warningData.RegisterStack?.ToString() ?? "(none)\n",
                            warningData.LastPingTime.ToString("u"), warningData.LastPingStack?.ToString() ?? "(none)\n",
                            warningData.LastPingThread?.Name ?? "(none)");
                        Log.Watchdog.TraceEvent(TraceEventType.Warning, 0, message);
                    } else {
                        break;
                    }
                }

                // Log all critical timeouts
                foreach (string critical in timeouts) {
                    isCritical = true;
                    if (Log.Watchdog.Switch.ShouldTrace(TraceEventType.Error)) {
                        WatchdogData criticalData = m_Watchdog[critical];
                        string message = string.Format(
                            "Watchdog TIMEOUT: '{0}' timeout {1}ms\n" +
                            " Registered at {2}: Stack:\n{3}" +
                            " Last ping at {4}: Stack:\n{5}" +
                            " Thread {6}",
                            critical, criticalData.CriticalTimeout,
                            criticalData.RegisterTime.ToString("u"), criticalData.RegisterStack?.ToString() ?? "(none)\n",
                            criticalData.LastPingTime.ToString("u"), criticalData.LastPingStack?.ToString() ?? "(none)\n",
                            criticalData.LastPingThread?.Name ?? "(none)");
                        Log.Watchdog.TraceEvent(TraceEventType.Error, 0, message);
                    } else {
                        break;
                    }
                }

                if (isCritical) {
                    // Crash dump and exit the program
                    OnCriticalEvent(this, new WatchdogEventArgs(timeouts));
                } else if (isWarning) {
                    // Crash dump
                    OnWarningEvent(this, new WatchdogEventArgs(warnings));
                }
                m_Timer.SetDelay(m_Watchdog.GetNextExpiry());
            }
        }

        /// <summary>
        /// Occurs when a watchdog warning has occurred.
        /// </summary>
        public event EventHandler<WatchdogEventArgs> WarningEvent;

        private void OnWarningEvent(object sender, WatchdogEventArgs args)
        {
            EventHandler<WatchdogEventArgs> handler = WarningEvent;
            if (handler != null) {
                handler(sender, args);
            } else {
                DefaultWarningEvent(sender, args);
            }
        }

        /// <summary>
        /// Performs default behavior on a watchdog warning.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// If you do not register to <see cref="WarningEvent"/>, the default behavior is to call this method. Else
        /// you might want to register your own event and then call this method in addition.
        /// </remarks>
        public static void DefaultWarningEvent(object sender, WatchdogEventArgs args)
        {
            string prefix = string.Format("{0}.wd", Process.GetCurrentProcess().ProcessName);
            string fileName = Dump.Crash.Data.GetCrashPath(prefix);
            try {
                string path = CrashReporter.CreateDump(fileName, Dump.CoreType.None);
                Log.CrashLog.TraceEvent(TraceEventType.Verbose, 0, "Watchdog warning created at: {0}", path);
            } catch {
                Log.CrashLog.TraceEvent(TraceEventType.Information, 0, "Watchdog warning failed");
            }
        }

        /// <summary>
        /// Occurs when a watchdog timeout has occurred.
        /// </summary>
        public event EventHandler<WatchdogEventArgs> CriticalEvent;

        private void OnCriticalEvent(object sender, WatchdogEventArgs args)
        {
            EventHandler<WatchdogEventArgs> handler = CriticalEvent;
            if (handler != null) {
                handler(sender, args);
            } else {
                DefaultCriticalEvent(sender, args);
            }
        }

        /// <summary>
        /// Performs default behavior on a watchdog timeout.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// If you do not register to <see cref="CriticalEvent"/>, the default behavior is to call this method. Else
        /// you might want to register your own event and then call this method in addition.
        /// </remarks>
        public static void DefaultCriticalEvent(object sender, WatchdogEventArgs args)
        {
            string prefix = string.Format("{0}.wderr", Process.GetCurrentProcess().ProcessName);
            string fileName = Dump.Crash.Data.GetCrashPath(prefix);
            try {
                string path = CrashReporter.CreateDump(fileName);
                Log.CrashLog.TraceEvent(TraceEventType.Information, 0, "Watchdog error created at: {0}", path);
            } catch {
                Log.CrashLog.TraceEvent(TraceEventType.Error, 0, "Watchdog error failed");
            }
            Environment.Exit(-1);
        }
    }
}
