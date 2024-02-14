namespace RJCP.Diagnostics.Crash.Dumpers
{
    using Crash.Export;
    using NUnit.Framework;
#if !NET40_LEGACY
    using System.Threading.Tasks;
#endif

    [TestFixture]
    public class TestBlockTest
    {
        [Test]
        public void DumpTestBlock()
        {
            using (MemoryCrashDataDumpFile dump = new()) {
                ICrashDataExport testBlock = new TestBlock();
                testBlock.Dump(dump);
                dump.Flush();

                Assert.That(CheckDumpTestBlock(dump), Is.True);
            }
        }

#if !NET40_LEGACY
        [Test]
        public async Task DumpTestBlockAsync()
        {
            using (MemoryCrashDataDumpFile dump = new()) {
                ICrashDataExport testBlock = new TestBlock();
                await testBlock.DumpAsync(dump);
                await dump.FlushAsync();

                Assert.That(CheckDumpTestBlock(dump), Is.True);
            }
        }
#endif

        private static bool CheckDumpTestBlock(MemoryCrashDataDumpFile dump)
        {
            Assert.That(dump["TestBlock"].Table, Has.Count.EqualTo(1));
            Assert.That(dump["TestBlock"].Table[0].Row[0].Field["Property"], Is.EqualTo("TestProperty"));
            Assert.That(dump["TestBlock"].Table[0].Row[0].Field["Value"], Is.EqualTo("TestValue"));
            return true;
        }
    }
}
