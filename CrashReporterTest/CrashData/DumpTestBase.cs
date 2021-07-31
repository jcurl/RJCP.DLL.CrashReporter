namespace RJCP.Diagnostics.CrashData
{
    using CrashExport;
    using NUnit.Framework;
#if NET45_OR_GREATER
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

                dumpFile.DumpContent();
                Assert.That(CheckDump(dumpFile), Is.True);
            }
        }

        protected virtual bool CheckDump(MemoryCrashDataDumpFile dumpFile)
        {
            Assert.That(dumpFile[TableName].Table.Count, Is.Not.EqualTo(0));
            return true;
        }

#if NET45_OR_GREATER
        [Test]
        public async Task DumpTestAsync()
        {
            using (MemoryCrashDataDumpFile dumpFile = new MemoryCrashDataDumpFile()) {
                ICrashDataExport dump = GetDumper();
                await dump.DumpAsync(dumpFile);
                await dumpFile.FlushAsync();

                dumpFile.DumpContent();
                Assert.That(CheckDump(dumpFile), Is.True);
            }
        }
#endif
    }
}
