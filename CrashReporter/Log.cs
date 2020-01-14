namespace RJCP.Diagnostics
{
    using System.Diagnostics;

    internal static class Log
    {
        public readonly static TraceSource CrashLog = new TraceSource("RJCP.CrashReporter");
    }
}
