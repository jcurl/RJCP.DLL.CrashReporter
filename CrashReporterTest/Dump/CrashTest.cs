namespace RJCP.Diagnostics.Dump
{
    using System.IO;
    using CrashExport;
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

        [Test]
        public void AsynchronousDump()
        {
            // Set up our test factory to allow asynchronous dumps. The implementation may, or may not, do the dump
            // asynchronously.

            ICrashDumpFactory origFactory = Crash.Data.CrashDumpFactory;
            MemoryCrashDumpFactory factory = new MemoryCrashDumpFactory {
                IsSynchronous = false
            };
            Crash.Data.CrashDumpFactory = factory;

            try {
                string path = Crash.Data.Dump();
                Assert.That(path, Is.Not.Null);
                if (Directory.Exists(path)) Deploy.DeleteDirectory(path);
            } finally {
                // The factory is a singleton, so it must be restored for default behaviour with other test cases.
                Crash.Data.CrashDumpFactory = origFactory;
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

        [Test]
        public async Task AsynchronousDumpAsync()
        {
            // Set up our test factory to allow asynchronous dumps
            ICrashDumpFactory origFactory = Crash.Data.CrashDumpFactory;
            MemoryCrashDumpFactory factory = new MemoryCrashDumpFactory {
                IsSynchronous = false
            };
            Crash.Data.CrashDumpFactory = factory;

            try {
                string path = await Crash.Data.DumpAsync();
                Assert.That(path, Is.Not.Null);
                if (Directory.Exists(path)) Deploy.DeleteDirectory(path);
            } finally {
                // The factory is a singleton, so it must be restored for default behaviour with other test cases.
                Crash.Data.CrashDumpFactory = origFactory;
            }
        }
#endif
    }
}
