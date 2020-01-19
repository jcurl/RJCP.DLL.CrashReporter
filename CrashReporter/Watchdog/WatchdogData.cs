namespace RJCP.Diagnostics.Watchdog
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    internal class WatchdogData
    {
        public int WarningTimeout { get; set; }

        public int CriticalTimeout { get; set; }

        public DateTime RegisterTime { get; set; }

        public Thread RegisterThread { get; set; }

        public StackTrace RegisterStack { get; set; }

        public DateTime LastPingTime { get; set; }

        public Thread LastPingThread { get; set; }

        public StackTrace LastPingStack { get; set; }
    }
}
