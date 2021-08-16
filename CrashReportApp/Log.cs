namespace CrashReportApp
{
    using System.Diagnostics;
    using RJCP.Diagnostics.Trace;

    public static class Log
    {
        public static LogSource App { get; private set; }

        static Log()
        {
#if NETCOREAPP
            SimplePrioMemoryLog log = new SimplePrioMemoryLog() {
                Critical = 100,
                Error = 150,
                Warning = 200,
                Info = 250,
                Verbose = 300,
                Other = 100,
                Total = 1500
            };
            MemoryTraceListener listener = new MemoryTraceListener(log);
            LogSource.SetLogSource("CrashReporterApp", SourceLevels.Verbose, listener);
            LogSource.SetLogSource("RJCP.CrashReporter", SourceLevels.Verbose, listener);
            LogSource.SetLogSource("RJCP.CrashReporter.Watchdog", SourceLevels.Verbose, listener);
#endif
            App = new LogSource("CrashReporterApp");
        }
    }
}
