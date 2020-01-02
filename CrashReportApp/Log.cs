namespace CrashReportApp
{
    using System.Diagnostics;

    public static class Log
    {
        public static TraceSource App { get; }  = new TraceSource("CrashReporterApp");
    }
}
