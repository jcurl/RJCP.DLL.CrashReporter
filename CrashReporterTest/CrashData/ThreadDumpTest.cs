namespace RJCP.Diagnostics.CrashData
{
    using CrashExport;
    using NUnit.Framework;
#if NET45
    using System.Threading.Tasks;
#endif

    [TestFixture(Category = "CrashReporter.CrashData")]
    public class ThreadDumpTest
    {
        [Test]
        public void ThreadDump()
        {
            using (MemoryCrashDataDumpFile dumpFile = new MemoryCrashDataDumpFile()) {
                ThreadDump dump = new ThreadDump();
                dump.Dump(dumpFile);
                dumpFile.Flush();

                Assert.That(CheckDump(dumpFile), Is.True);
            }
        }

        private bool CheckDump(MemoryCrashDataDumpFile dumpFile)
        {
            dumpFile.DumpContent();
            Assert.That(dumpFile["OSThreads"].Table.Count, Is.Not.EqualTo(0));
            return true;
        }

#if NET45
        [Test]
        public async Task ThreadDumpAsync()
        {
            using (MemoryCrashDataDumpFile dumpFile = new MemoryCrashDataDumpFile()) {
                ThreadDump dump = new ThreadDump();
                await dump.DumpAsync(dumpFile);
                await dumpFile.FlushAsync();

                Assert.That(CheckDump(dumpFile), Is.True);
            }
        }
#endif
    }
}
