namespace RJCP.Diagnostics.Trace
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Crash;
    using Crash.Export;
    using NUnit.Framework;
    using NUnit.Framework.Internal;
    using RJCP.CodeQuality.NUnitExtensions;

    [TestFixture]
    public class AppConfigLogTest
    {
        private static TraceSource GetTraceSource(string traceSourceName)
        {
            // this method is required for .NET Core, as the TraceSource doesn't read a .NET App.Config file to
            // instantiate itself.
            TraceSource traceSource = new TraceSource(traceSourceName);
#if NET6_0_OR_GREATER
            traceSource.Switch = new SourceSwitch(traceSourceName, "Verbose");
            traceSource.Listeners.Clear();

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
            traceSource.Listeners.Add(listener);
#endif
            return traceSource;
        }

        [Test]
        public void LogData()
        {
            TraceSource source1 = GetTraceSource("RJCP.CrashReporterTest");
            source1.TraceEvent(TraceEventType.Verbose, 0, "Message");

            TraceSource source2 = GetTraceSource("RJCP.CrashReporterTest2");
            source2.TraceEvent(TraceEventType.Warning, 0, "Message");

            MemoryTraceListener log1 = null;
            foreach (var listenerObject in source1.Listeners) {
                if (log1 == null) log1 = listenerObject as MemoryTraceListener;
            }
            Assert.That(log1, Is.Not.Null);

            MemoryTraceListener log2 = null;
            foreach (var listenerObject in source2.Listeners) {
                if (log2 == null) log2 = listenerObject as MemoryTraceListener;
            }
            Assert.That(log2, Is.Not.Null);

            // Now dump it. .NET will only instantiate the trace source once according to the app.config.
            using (MemoryCrashDataDumpFile dumpFile = new MemoryCrashDataDumpFile()) {
                MemoryTraceListenerAccessor accessor = new MemoryTraceListenerAccessor(log1);
                accessor.MemoryLogDump.Dump(dumpFile);
                dumpFile.Flush();

                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.GreaterThan(0));
                Console.WriteLine("Count={0}", dumpFile["TraceListenerLog"].Table[0].Row.Count);
            }
        }

        [Test]
        public void LogDump()
        {
            TraceSource source = GetTraceSource("RJCP.CrashReporterTest");
            source.TraceEvent(TraceEventType.Warning, 0, "Warning message");

            using (ScratchPad scratch = Deploy.ScratchPad(ScratchOptions.CreateScratch | ScratchOptions.KeepCurrentDir)) {
                CrashData.Instance.Dump(Path.Combine(scratch.Path, CrashData.Instance.CrashDumpFactory.FileName));
                Assert.That(File.Exists(Path.Combine(scratch.Path, CrashData.Instance.CrashDumpFactory.FileName)), Is.True);
            }
        }
    }
}
