namespace RJCP.Diagnostics.CrashData
{
    using CrashExport;
    using NUnit.Framework;
#if NET45
    using System.Threading.Tasks;
#endif

    [TestFixture(Category = "CrashReporter.CrashData")]
    public class NetVersionDumpTest
    {
        [Test]
        public void NetVersionDump()
        {
            using (MemoryCrashDataDumpFile dumpFile = new MemoryCrashDataDumpFile()) {
                NetVersionDump dump = new NetVersionDump();
                dump.Dump(dumpFile);
                dumpFile.Flush();

                Assert.That(CheckDump(dumpFile), Is.True);
            }
        }

        private bool CheckDump(MemoryCrashDataDumpFile dumpFile)
        {
            dumpFile.DumpContent();
            Assert.That(dumpFile["NetVersionInstalled"].Table.Count, Is.Not.EqualTo(0));
            return true;
        }

#if NET45
        [Test]
        public async Task NetVersionDumpAsync()
        {
            using (MemoryCrashDataDumpFile dumpFile = new MemoryCrashDataDumpFile()) {
                NetVersionDump dump = new NetVersionDump();
                await dump.DumpAsync(dumpFile);
                await dumpFile.FlushAsync();

                Assert.That(CheckDump(dumpFile), Is.True);
            }
        }
#endif
    }
}
