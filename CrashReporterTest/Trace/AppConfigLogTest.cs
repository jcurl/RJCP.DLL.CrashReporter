namespace RJCP.Diagnostics.Trace
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using NUnit.Framework;
    using CrashExport;
    using Dump;
    using RJCP.CodeQuality.NUnitExtensions;

    [TestFixture(Category = "CrashReporter.Trace")]
    public class AppConfigLogTest
    {
        [Test]
        public void LogData()
        {
            TraceSource source1 = new TraceSource("RJCP.CrashReporterTest");
            source1.TraceEvent(TraceEventType.Verbose, 0, "Message");

            TraceSource source2 = new TraceSource("RJCP.CrashReporterTest2");
            source2.TraceEvent(TraceEventType.Warning, 0, "Message");

            ICrashDataExport log1 = null;
            foreach (var listenerObject in source1.Listeners) {
                if (log1 == null) log1 = listenerObject as ICrashDataExport;
            }
            Assert.That(log1, Is.Not.Null);

            ICrashDataExport log2 = null;
            foreach (var listenerObject in source1.Listeners) {
                if (log2 == null) log2 = listenerObject as ICrashDataExport;
            }
            Assert.That(log2, Is.Not.Null);

            // Now dump it. .NET will only instantiate the trace source once according to the app.config.
            using (MemoryCrashDataDumpFile dumpFile = new MemoryCrashDataDumpFile()) {
                log1.Dump(dumpFile);
                dumpFile.Flush();

                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.GreaterThan(0));
                Console.WriteLine("Count={0}", dumpFile["TraceListenerLog"].Table[0].Row.Count);
            }
        }

        [Test]
        public void LogDump()
        {
            TraceSource source = new TraceSource("RJCP.CrashReporterTest");
            source.TraceEvent(TraceEventType.Warning, 0, "Warning message");

            using (ScratchPad scratch = Deploy.ScratchPad(ScratchOptions.CreateScratch | ScratchOptions.KeepCurrentDir)) {
                Crash.Data.Dump(Path.Combine(scratch.Path, Crash.Data.CrashDumpFactory.FileName));
                Assert.That(File.Exists(Path.Combine(scratch.Path, Crash.Data.CrashDumpFactory.FileName)), Is.True);
            }
        }
    }
}
