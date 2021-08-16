namespace RJCP.Diagnostics.Watchdog
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using Config.CrashReporter;

    /// <summary>
    /// A Watchdog for threads.
    /// </summary>
    public sealed class ThreadWatchdog : IWatchdog
    {
        private readonly WatchdogList m_Watchdog;
        private readonly Timer.ISingleShotTimer m_Timer;

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
        /// <paramref name="timerSource"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="timer"/> is be <see langword="null"/>.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
        public bool Register(string name, int warning, int critical)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            bool overrideActive = CrashReporter.Config.Watchdog.Overrides.TryGetOverride(name, out WatchdogOverride wdOverride);
            if (overrideActive) {
                warning = wdOverride.WarningTimeout;
                critical = wdOverride.CriticalTimeout;
            }

            lock (m_TimerSyncLock) {
                bool added = m_Watchdog.Add(name, warning, critical);
                if (added) {
                    m_Timer.SetDelay(m_Watchdog.GetNextExpiry());
                    Log.Watchdog.TraceEvent(TraceEventType.Information,
                        "Watchdog Register: '{0}'; warning {1}ms; critical {2}ms{3}",
                        name, warning, critical, overrideActive ? " (override)" : "");
                } else {
                    if (Log.Watchdog.ShouldTrace(TraceEventType.Warning)) {
                        WatchdogData registerData = m_Watchdog[name];
                        string message = string.Format(
                            "Watchdog Register: '{0}' already registered\n" +
                            " Registered at {1}: Stack:\n{2}" +
                            " Call Stack:\n{3}",
                            name, registerData.RegisterTime.ToString("u"), registerData.RegisterStack?.ToString() ?? "(none)",
                            new StackFrame(true)?.ToString() ?? "(none)");
                        Log.Watchdog.TraceEvent(TraceEventType.Warning, message);
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
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
        public bool Unregister(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            lock (m_TimerSyncLock) {
                bool removed = m_Watchdog.Remove(name);
                if (removed) {
                    m_Timer.SetDelay(m_Watchdog.GetNextExpiry());
                    Log.Watchdog.TraceEvent(TraceEventType.Information, "Watchdog Unregister: '{0}'", name);
                } else {
                    if (Log.Watchdog.ShouldTrace(TraceEventType.Warning)) {
                        string message = string.Format(
                            "Watchdog Unregister: '{0}' already unregistered\n" +
                            " Call Stack:\n{1}",
                            name, new StackFrame(true)?.ToString() ?? "(none)");
                        Log.Watchdog.TraceEvent(TraceEventType.Warning, message);
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
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
        public bool Ping(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            lock (m_TimerSyncLock) {
                bool ping = m_Watchdog.Reset(name, CrashReporter.Config.Watchdog.Ping.StackCapture);
                if (ping) {
                    m_Timer.SetDelay(m_Watchdog.GetNextExpiry());
                    Log.Watchdog.TraceEvent(TraceEventType.Verbose,
                        "Watchdog Ping: '{0}'", name);
                } else {
                    if (Log.Watchdog.ShouldTrace(TraceEventType.Warning)) {
                        string message = string.Format(
                            "Watchdog Ping: '{0}' not registered\n" +
                            " Call Stack:{1}" +
                            " Thread {2}",
                            name, GetStack(new StackTrace(true)), GetThread(Thread.CurrentThread));
                        Log.Watchdog.TraceEvent(TraceEventType.Warning, message);
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
                    if (Log.Watchdog.ShouldTrace(TraceEventType.Warning)) {
                        WatchdogData warningData = m_Watchdog[warning];
                        string message = string.Format(
                            "Watchdog warning: '{0}' timeout {1}ms\n" +
                            " Registered at {2}: Stack:{3}" +
                            " Last ping at {4}: Stack:{5}" +
                            " Thread {6}",
                            warning, warningData.WarningTimeout,
                            warningData.RegisterTime.ToString("u"), GetStack(warningData.RegisterStack),
                            warningData.LastPingTime.ToString("u"), GetStack(warningData.LastPingStack),
                            GetThread(warningData.LastPingThread));
                        Log.Watchdog.TraceEvent(TraceEventType.Warning, message);
                    } else {
                        break;
                    }
                }

                // Log all critical timeouts
                foreach (string critical in timeouts) {
                    isCritical = true;
                    if (Log.Watchdog.ShouldTrace(TraceEventType.Error)) {
                        WatchdogData criticalData = m_Watchdog[critical];
                        string message = string.Format(
                            "Watchdog TIMEOUT: '{0}' timeout {1}ms\n" +
                            " Registered at {2}: Stack:{3}" +
                            " Last ping at {4}: Stack:{5}" +
                            " Thread {6}",
                            critical, criticalData.CriticalTimeout,
                            criticalData.RegisterTime.ToString("u"), GetStack(criticalData.RegisterStack),
                            criticalData.LastPingTime.ToString("u"), GetStack(criticalData.LastPingStack),
                            GetThread(criticalData.LastPingThread));
                        Log.Watchdog.TraceEvent(TraceEventType.Error, message);
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

        private static string GetStack(StackTrace stackTrace)
        {
            if (stackTrace == null) return " (none)\n";
            return string.Format("\n{0}", stackTrace.ToString());
        }

        private static string GetThread(Thread thread)
        {
            if (thread == null) return "(none)";
            if (thread.Name == null)
                return thread.ManagedThreadId.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return string.Format("{0} ({1})", thread.Name, thread.ManagedThreadId);
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
            string dumpPath = Dump.Crash.GetCrashDir(prefix);
            try {
                string path = CrashReporter.CreateDump(dumpPath, Dump.CoreType.None);
                Log.CrashLog.TraceEvent(TraceEventType.Verbose, "Watchdog warning created at: {0}", path);
            } catch {
                Log.CrashLog.TraceEvent(TraceEventType.Information, "Watchdog warning failed");
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
            string dumpPath = Dump.Crash.GetCrashDir(prefix);
            try {
                string path = CrashReporter.CreateDump(dumpPath);
                Log.CrashLog.TraceEvent(TraceEventType.Information, "Watchdog error created at: {0}", path);
            } catch {
                Log.CrashLog.TraceEvent(TraceEventType.Error, "Watchdog error failed");
            }
            Environment.Exit(-1);
        }
    }
}
