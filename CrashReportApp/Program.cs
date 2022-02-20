using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using RJCP.Diagnostics;
using RJCP.Diagnostics.Dump;

namespace CrashReportApp
{
    static class Program
    {
        static int Main(string[] args)
        {
            CrashReporter.SetExceptionHandlers();

            Log.App.TraceEvent(TraceEventType.Information, "Program Started");
            Log.App.TraceEvent(TraceEventType.Critical, "Crash Reporter App - Critical");
            Log.App.TraceEvent(TraceEventType.Error, "Crash Reporter App - Error");
            Log.App.TraceEvent(TraceEventType.Warning, "Crash Reporter App - Warning");
            Log.App.TraceEvent(TraceEventType.Information, "Crash Reporter App - Information");
            Log.App.TraceEvent(TraceEventType.Verbose, "Crash Reporter App - Verbose");
            Log.App.TraceEvent(TraceEventType.Start, "Crash Reporter App - Start");
            Log.App.TraceEvent(TraceEventType.Stop, "Crash Reporter App - Stop");
            Log.App.TraceEvent(TraceEventType.Suspend, "Crash Reporter App - Suspend");
            Log.App.TraceEvent(TraceEventType.Resume, "Crash Reporter App - Resume");
            Log.App.TraceEvent(TraceEventType.Transfer, "Crash Reporter App - Transfer");

            if (args.Length != 1 || !Enum.TryParse(args[0], true, out ExecutionMode mode)) {
#if NETCOREAPP
                // If the user logs via the console, we need to wait before exiting. .NET Core ConsoleLogger has a
                // thread running in the background that delays the output to the console, which might not print if the
                // program exits too quickly.
                Thread.Sleep(200);
#endif
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
