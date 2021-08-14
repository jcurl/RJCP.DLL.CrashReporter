namespace RJCP.Diagnostics.Trace
{
    using CrashExport;
    using NUnit.Framework;
#if NET45_OR_GREATER || NETCOREAPP
    using System.Threading.Tasks;
#endif

    [TestFixture(Category = "CrashReporter.Trace")]
    public class MemoryListListenerTest
    {
        [Test]
        public void Initialize()
        {
            MemoryListListener listener = new MemoryListListener();

            using (MemoryCrashDataDumpFile dumpFile = new MemoryCrashDataDumpFile()) {
                listener.Dump(dumpFile);
                dumpFile.Flush();

                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void WriteWithNoNewLines()
        {
            MemoryListListener listener = new MemoryListListener();
            listener.Write("Single Line");

            using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void WriteWithNewLine()
        {
            MemoryListListener listener = new MemoryListListener();
            listener.Write("Single Line\nLine2");

            using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(1));
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["message"], Is.EqualTo("Single Line"));
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["eventType"], Is.EqualTo("Verbose"));
            }
        }

        [Test]
        public void WriteWithNewLine2()
        {
            MemoryListListener listener = new MemoryListListener();
            listener.Write("\nSingle Line\nLine2");

            using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(2));
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["message"], Is.EqualTo(""));
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[1].Field["message"], Is.EqualTo("Single Line"));
            }
        }

        [Test]
        public void WriteEmptyString()
        {
            MemoryListListener listener = new MemoryListListener();
            listener.Write("");

            using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void WriteTwice()
        {
            MemoryListListener listener = new MemoryListListener();
            listener.Write("Single Line: ");
            listener.Write("Continuation");

            using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void WriteTwiceFlush()
        {
            MemoryListListener listener = new MemoryListListener();
            listener.Write("Single Line: ");
            listener.Write("Continuation");
            listener.Flush();

            using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(1));
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["message"], Is.EqualTo("Single Line: Continuation"));
            }
        }

        [Test]
        public void WriteThenWriteLine()
        {
            MemoryListListener listener = new MemoryListListener();
            listener.Write("Single Line: ");
            listener.WriteLine("Continuation");

            using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(1));
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["message"], Is.EqualTo("Single Line: Continuation"));
            }
        }

        [Test]
        public void WriteLine()
        {
            MemoryListListener listener = new MemoryListListener();
            listener.WriteLine("Single Line");

            using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(1));
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["message"], Is.EqualTo("Single Line"));
            }
        }

        [Test]
        public void WriteLineWithNewLine()
        {
            MemoryListListener listener = new MemoryListListener();
            listener.WriteLine("Single Line\nLine2");

            using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(2));
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["message"], Is.EqualTo("Single Line"));
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[1].Field["message"], Is.EqualTo("Line2"));
            }
        }

        [Test]
        public void WriteLineWithNewLine2()
        {
            MemoryListListener listener = new MemoryListListener();
            listener.WriteLine("\nSingle Line\nLine2");

            using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(3));
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["message"], Is.EqualTo(""));
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[1].Field["message"], Is.EqualTo("Single Line"));
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[2].Field["message"], Is.EqualTo("Line2"));
            }
        }

        [Test]
        public void WriteLineEmptyString()
        {
            MemoryListListener listener = new MemoryListListener();
            listener.WriteLine("");

            using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(1));
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["message"], Is.EqualTo(""));
            }
        }

        [Test]
        public void WriteLineTwice()
        {
            MemoryListListener listener = new MemoryListListener();
            listener.WriteLine("Single Line: ");
            listener.WriteLine("Continuation");

            using (MemoryCrashDataDumpFile dumpFile = Dump(listener)) {
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(2));
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["message"], Is.EqualTo("Single Line: "));
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[1].Field["message"], Is.EqualTo("Continuation"));
            }
        }

        private static MemoryCrashDataDumpFile Dump(MemoryListListener listener)
        {
            MemoryCrashDataDumpFile dumpFile = null;
            try {
                dumpFile = new MemoryCrashDataDumpFile();
                listener.Dump(dumpFile);
                dumpFile.Flush();
            } catch {
                if (dumpFile != null) {
                    dumpFile.Flush();
                    dumpFile.Dispose();
                }
                throw;
            }
            return dumpFile;
        }

#if NET45_OR_GREATER || NETCOREAPP
        [Test]
        public async Task DumpAsync()
        {
            MemoryListListener listener = new MemoryListListener();
            listener.WriteLine("Line1");
            listener.Write("Test Line: ");
            listener.WriteLine("Done.");

            using (MemoryCrashDataDumpFile dumpFile = new MemoryCrashDataDumpFile()) {
                await listener.DumpAsync(dumpFile);
                await dumpFile.FlushAsync();

                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row.Count, Is.EqualTo(2));
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[0].Field["message"], Is.EqualTo("Line1"));
                Assert.That(dumpFile["TraceListenerLog"].Table[0].Row[1].Field["message"], Is.EqualTo("Test Line: Done."));
            }
        }
#endif
    }
}
