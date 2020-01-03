namespace RJCP.Diagnostics.CrashData
{
    using CrashExport;
    using NUnit.Framework;
#if NET45
    using System.Threading.Tasks;
#endif

    [TestFixture(Category = "CrashReporter.CrashData")]
    public abstract class DumpTestBase
    {
        protected abstract ICrashDataExport GetDumper();

        protected abstract string TableName { get; }

        [Test]
        public void DumpTest()
        {
            using (MemoryCrashDataDumpFile dumpFile = new MemoryCrashDataDumpFile()) {
                ICrashDataExport dump = GetDumper();
                dump.Dump(dumpFile);
                dumpFile.Flush();

                Assert.That(CheckDump(dumpFile), Is.True);
            }
        }

        private bool CheckDump(MemoryCrashDataDumpFile dumpFile)
        {
            dumpFile.DumpContent();
            Assert.That(dumpFile[TableName].Table.Count, Is.Not.EqualTo(0));
            return true;
        }

#if NET45
        [Test]
        public async Task DumpTestAsync()
        {
            using (MemoryCrashDataDumpFile dumpFile = new MemoryCrashDataDumpFile()) {
                ICrashDataExport dump = GetDumper();
                await dump.DumpAsync(dumpFile);
                await dumpFile.FlushAsync();

                Assert.That(CheckDump(dumpFile), Is.True);
            }
        }
#endif
    }
}
