using System;
using System.IO;
using RJCP.Diagnostics;
using RJCP.Diagnostics.Dump;

namespace CrashReportApp
{
    static class Program
    {
        static void Main()
        {
            CrashReporter.Source = Log.App;
            CrashReporter.SetExceptionHandlers();

            Log.App.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, "Program Started");
            string path = Path.Combine(Environment.CurrentDirectory, Crash.Data.CrashDumpFactory.FileName);
            Crash.Data.Dump(path);

            // Now simulate an unhandled exception
            throw new InvalidOperationException("An exception which should cause a dump");
        }
    }
}
