﻿namespace RJCP.Diagnostics.Trace
{
    using Crash;
    using Crash.Export;
    using NUnit.Framework;
#if !NET40_LEGACY
    using System.Threading.Tasks;
#endif

    [TestFixture]
    public class MemoryListListenerTest
    {
        [Test]
        public void Initialize()
        {
            using (MemoryCrashDataDumpFile dumpFile = new()) {
                int providers = CrashData.Instance.Providers.Count;

                using (MemoryListListener listener = new()) {
                    // Should be registered to the global crash handler
                    Assert.That(CrashData.Instance.Providers, Has.Count.EqualTo(providers + 1));

                    bool found = false;
                    ICrashDataExport dumper = GetCrashDataExport(listener);
                    foreach (ICrashDataExport item in CrashData.Instance.Providers) {
                        // Need to compare against the real object, not the wrapped object
                        if (item == ((MemoryLogDumpAccessor)dumper).PrivateTargetObject)
                            found = true;
                    }
                    Assert.That(found, Is.True, "Couldn't find listener registered to crash handler dumper");

                    // But now we can use the wrapped object
                    dumper.Dump(dumpFile);
                    dumpFile.Flush();

                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(0));
                }
                Assert.That(CrashData.Instance.Providers, Has.Count.EqualTo(providers));
            }
        }

        [Test]
        public void WriteWithNoNewLines()
        {
            using (MemoryListListener listener = new()) {
                listener.Write("Single Line");

                using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(0));
                }
            }
        }

        [Test]
        public void WriteWithNewLine()
        {
            using (MemoryListListener listener = new()) {
                listener.Write("Single Line\nLine2");

                using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(1));
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["message"], Is.EqualTo("Single Line"));
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["eventType"], Is.EqualTo("Verbose"));
                }
            }
        }

        [Test]
        public void WriteWithNewLine2()
        {
            using (MemoryListListener listener = new()) {
                listener.Write("\nSingle Line\nLine2");

                using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(2));
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["message"], Is.EqualTo(""));
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[1].Field["message"], Is.EqualTo("Single Line"));
                }
            }
        }

        [Test]
        public void WriteEmptyString()
        {
            using (MemoryListListener listener = new()) {
                listener.Write("");

                using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(0));
                }
            }
        }

        [Test]
        public void WriteTwice()
        {
            using (MemoryListListener listener = new()) {
                listener.Write("Single Line: ");
                listener.Write("Continuation");

                using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(0));
                }
            }
        }

        [Test]
        public void WriteTwiceFlush()
        {
            using (MemoryListListener listener = new()) {
                listener.Write("Single Line: ");
                listener.Write("Continuation");
                listener.Flush();

                using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(1));
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["message"], Is.EqualTo("Single Line: Continuation"));
                }
            }
        }

        [Test]
        public void WriteThenWriteLine()
        {
            using (MemoryListListener listener = new()) {
                listener.Write("Single Line: ");
                listener.WriteLine("Continuation");

                using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(1));
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["message"], Is.EqualTo("Single Line: Continuation"));
                }
            }
        }

        [Test]
        public void WriteLine()
        {
            using (MemoryListListener listener = new()) {
                listener.WriteLine("Single Line");

                using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(1));
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["message"], Is.EqualTo("Single Line"));
                }
            }
        }

        [Test]
        public void WriteLineWithNewLine()
        {
            using (MemoryListListener listener = new()) {
                listener.WriteLine("Single Line\nLine2");

                using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(2));
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["message"], Is.EqualTo("Single Line"));
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[1].Field["message"], Is.EqualTo("Line2"));
                }
            }
        }

        [Test]
        public void WriteLineWithNewLine2()
        {
            using (MemoryListListener listener = new()) {
                listener.WriteLine("\nSingle Line\nLine2");

                using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(3));
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["message"], Is.EqualTo(""));
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[1].Field["message"], Is.EqualTo("Single Line"));
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[2].Field["message"], Is.EqualTo("Line2"));
                }
            }
        }

        [Test]
        public void WriteLineEmptyString()
        {
            using (MemoryListListener listener = new()) {
                listener.WriteLine("");

                using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(1));
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["message"], Is.EqualTo(""));
                }
            }
        }

        [Test]
        public void WriteLineTwice()
        {
            using (MemoryListListener listener = new()) {
                listener.WriteLine("Single Line: ");
                listener.WriteLine("Continuation");

                using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(2));
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["message"], Is.EqualTo("Single Line: "));
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[1].Field["message"], Is.EqualTo("Continuation"));
                }
            }
        }

        private static ICrashDataExport GetCrashDataExport(MemoryTraceListener listener)
        {
            MemoryTraceListenerAccessor listenerAccessor = new(listener);
            return listenerAccessor.MemoryLogDump;
        }

        private static MemoryCrashDataDumpFile Dump(MemoryListListener listener)
        {
            MemoryCrashDataDumpFile dumpFile = null;
            try {
                dumpFile = new MemoryCrashDataDumpFile();
                ICrashDataExport dumper = GetCrashDataExport(listener);
                dumper.Dump(dumpFile);
                dumpFile.Flush();
            } catch {
                if (dumpFile is not null) {
                    dumpFile.Flush();
                    dumpFile.Dispose();
                }
                throw;
            }
            return dumpFile;
        }

#if !NET40_LEGACY
        [Test]
        public async Task DumpAsync()
        {
            using (MemoryListListener listener = new()) {
                listener.WriteLine("Line1");
                listener.Write("Test Line: ");
                listener.WriteLine("Done.");

                using (MemoryCrashDataDumpFile dumpFile = new()) {
                    ICrashDataExport dumper = GetCrashDataExport(listener);
                    await dumper.DumpAsync(dumpFile);
                    await dumpFile.FlushAsync();

                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(2));
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["message"], Is.EqualTo("Line1"));
                    Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[1].Field["message"], Is.EqualTo("Test Line: Done."));
                }
            }
        }
#endif
    }
}
