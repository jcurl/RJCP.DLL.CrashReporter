using System;
using System.IO;
using RJCP.Diagnostics.Dump;

namespace CrashReportApp
{
    static class Program
    {
        static void Main()
        {
            Log.App.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, "Program Started");

            string path = Path.Combine(Environment.CurrentDirectory, Crash.Data.CrashDumpFactory.FileName);
            Crash.Data.Dump(path);
        }
    }
}
