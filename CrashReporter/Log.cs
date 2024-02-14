namespace RJCP.Diagnostics
{
    using Trace;

    internal static class Log
    {
        public readonly static LogSource CrashLog = new("RJCP.CrashReporter");
        public readonly static LogSource Watchdog = new("RJCP.CrashReporter.Watchdog");
    }
}
