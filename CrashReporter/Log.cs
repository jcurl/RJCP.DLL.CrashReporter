namespace RJCP.Diagnostics
{
    using System.Diagnostics;

    internal static class Log
    {
        public readonly static TraceSource CrashLog = new TraceSource("RJCP.CrashReporter");
        public readonly static TraceSource Watchdog = new TraceSource("RJCP.CrashReporter.Watchdog");
    }
}
