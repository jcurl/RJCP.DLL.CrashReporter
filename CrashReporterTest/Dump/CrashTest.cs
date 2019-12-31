namespace RJCP.Diagnostics.Dump
{
    using System.IO;
    using NUnit.Framework;
#if NET45
    using System.Threading.Tasks;
#endif

    [TestFixture(Category = "CrashReporter.Dump")]
    public class CrashTest
    {
        [Test]
        public void PerformCrashDump()
        {
            string fileName = Crash.Data.Dump();
            Assert.That(File.Exists(fileName), Is.True);

            Deploy.DeleteDirectory(Path.GetDirectoryName(fileName));
        }

        [Test]
        public void PerformCrashDumpLocalDir()
        {
            using (ScratchPad scratch = Deploy.ScratchPad(ScratchOptions.CreateScratch | ScratchOptions.KeepCurrentDir)) {
                Crash.Data.Dump(Path.Combine(scratch.Path, Crash.Data.CrashDumpFactory.FileName));

                Assert.That(File.Exists(Path.Combine(scratch.Path, Crash.Data.CrashDumpFactory.FileName)), Is.True);
            }
        }

#if NET45
        [Test]
        public async Task PerformCrashDumpAsync()
        {
            string fileName = await Crash.Data.DumpAsync();
            Assert.That(File.Exists(fileName), Is.True);

            Deploy.DeleteDirectory(Path.GetDirectoryName(fileName));
        }

        [Test]
        public async Task PerformCrashDumpLocalDirAsync()
        {
            using (ScratchPad scratch = Deploy.ScratchPad(ScratchOptions.CreateScratch | ScratchOptions.KeepCurrentDir)) {
                await Crash.Data.DumpAsync(Path.Combine(scratch.Path, Crash.Data.CrashDumpFactory.FileName));

                Assert.That(File.Exists(Path.Combine(scratch.Path, Crash.Data.CrashDumpFactory.FileName)), Is.True);
            }
        }
#endif
    }
}
