using System;
using System.IO;
using RJCP.Diagnostics;
using RJCP.Diagnostics.Dump;

namespace CrashReportApp
{
    static class Program
    {
        static int Main(string[] args)
        {
            CrashReporter.SetExceptionHandlers();

            Log.App.TraceEvent(System.Diagnostics.TraceEventType.Information, "Program Started");
            if (args.Length != 1 || !Enum.TryParse(args[0], true, out ExecutionMode mode)) {
                PrintHelp();
                return 0;
            }

            try {
                switch (mode) {
                case ExecutionMode.Exception:
                    // Now simulate an unhandled exception
                    throw new InvalidOperationException("An exception which should cause a dump");
                case ExecutionMode.Watchdog:
                    // Register a watchdog.
                    CrashReporter.Watchdog.Register("app", 2000, 5000);

                    // This should result in a watchdog timeout, and crash the program (unless there's an override)
                    CrashReporter.Watchdog.Ping("app");
                    System.Threading.Thread.Sleep(10000);
                    break;
                }
            } finally {
                string path = Path.Combine(Environment.CurrentDirectory, Crash.Data.CrashDumpFactory.FileName);
                Crash.Data.Dump(path);
            }

            return 0;
        }

        private static void PrintHelp()
        {
            Console.WriteLine("{0} <mode>", typeof(Program).Assembly.GetName().Name);
            Console.WriteLine("  <mode> can be one of the following:");
            Console.WriteLine("    exception: Simulate an unhandled exception");
            Console.WriteLine("    watchdog: simulate a watchdog timeout");
        }
    }
}
