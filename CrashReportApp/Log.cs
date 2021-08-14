namespace CrashReportApp
{
    using System.Diagnostics;
    using RJCP.Diagnostics.Trace;

    public static class Log
    {
        public static TraceSource App { get; } = new TraceSource("CrashReporterApp");

        static Log()
        {
#if NETCOREAPP
            App.Switch = new SourceSwitch("CrashReporterApp", "Verbose");
            App.Listeners.Clear();

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
            App.Listeners.Add(listener);
#endif
        }
    }
}
