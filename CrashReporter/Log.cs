namespace RJCP.Diagnostics
{
    using Trace;

    internal static class Log
    {
        public readonly static LogSource CrashLog = new LogSource("RJCP.CrashReporter");
        public readonly static LogSource Watchdog = new LogSource("RJCP.CrashReporter.Watchdog");
    }
}
