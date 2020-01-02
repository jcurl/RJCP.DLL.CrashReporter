namespace RJCP.Diagnostics.CrashData
{
    using CrashExport;
    using NUnit.Framework;
#if NET45
    using System.Threading.Tasks;
#endif

    [TestFixture(Category = "CrashReporter.CrashData")]
    public class NetworkDumpTest
    {
        [Test]
        public void NetworkDump()
        {
            using (MemoryCrashDataDumpFile dumpFile = new MemoryCrashDataDumpFile()) {
                NetworkDump dump = new NetworkDump();
                dump.Dump(dumpFile);
                dumpFile.Flush();

                Assert.That(CheckDump(dumpFile), Is.True);
            }
        }

        private bool CheckDump(MemoryCrashDataDumpFile dumpFile)
        {
            dumpFile.DumpContent();
            Assert.That(dumpFile["Network"].Table.Count, Is.Not.EqualTo(0));
            return true;
        }

#if NET45
        [Test]
        public async Task NetworkDumpAsync()
        {
            using (MemoryCrashDataDumpFile dumpFile = new MemoryCrashDataDumpFile()) {
                NetworkDump dump = new NetworkDump();
                await dump.DumpAsync(dumpFile);
                await dumpFile.FlushAsync();

                Assert.That(CheckDump(dumpFile), Is.True);
            }
        }
#endif
    }
}
