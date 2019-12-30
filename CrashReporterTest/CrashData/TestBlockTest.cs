namespace RJCP.Diagnostics.CrashData
{
    using CrashExport;
    using NUnit.Framework;
#if NET45
    using System.Threading.Tasks;
#endif

    [TestFixture(Category = "CrashReporter.CrashData")]
    public class TestBlockTest
    {
        [Test]
        public void DumpTestBlock()
        {
            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                ICrashDataExport testBlock = new TestBlock();
                testBlock.Dump(dump);
                dump.Flush();

                Assert.That(CheckDumpTestBlock(dump), Is.True);
            }
        }

#if NET45
        [Test]
        public async Task DumpTestBlockAsync()
        {
            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                ICrashDataExport testBlock = new TestBlock();
                await testBlock.DumpAsync(dump);
                await dump.FlushAsync();

                Assert.That(CheckDumpTestBlock(dump), Is.True);
            }
        }
#endif

        private static bool CheckDumpTestBlock(MemoryCrashDataDumpFile dump)
        {
            Assert.That(dump["TestBlock"].Count, Is.EqualTo(1));
            Assert.That(dump["TestBlock"][0]["Property"], Is.EqualTo("TestProperty"));
            Assert.That(dump["TestBlock"][0]["Value"], Is.EqualTo("TestValue"));
            return true;
        }
    }
}
