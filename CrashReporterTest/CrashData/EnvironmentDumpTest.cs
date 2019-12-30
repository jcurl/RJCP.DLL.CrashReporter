namespace RJCP.Diagnostics.CrashData
{
    using CrashExport;
    using NUnit.Framework;
#if NET45
    using System.Threading.Tasks;
#endif

    [TestFixture(Category = "CrashReporter.CrashData")]
    public class EnvironmentDumpTest
    {
        [Test]
        public void EnvVarDump()
        {
            using (MemoryCrashDataDumpFile dumpFile = new MemoryCrashDataDumpFile()) {
                EnvironmentDump dump = new EnvironmentDump();
                dump.Dump(dumpFile);
                dumpFile.Flush();

                Assert.That(CheckDump(dumpFile), Is.True);
            }
        }

        private bool CheckDump(MemoryCrashDataDumpFile dumpFile)
        {
            dumpFile.DumpContent();
            Assert.That(dumpFile["EnvironmentVariables"].Count, Is.Not.EqualTo(0));
            return true;
        }

#if NET45
        [Test]
        public async Task EnvVarDumpAsync()
        {
            using (MemoryCrashDataDumpFile dumpFile = new MemoryCrashDataDumpFile()) {
                EnvironmentDump dump = new EnvironmentDump();
                await dump.DumpAsync(dumpFile);
                await dumpFile.FlushAsync();

                Assert.That(CheckDump(dumpFile), Is.True);
            }
        }
#endif
    }
}
