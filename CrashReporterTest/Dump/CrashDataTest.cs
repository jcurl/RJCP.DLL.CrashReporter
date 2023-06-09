namespace RJCP.Diagnostics.Dump
{
    using System.IO;
    using CrashExport;
    using NUnit.Framework;
    using RJCP.CodeQuality.NUnitExtensions;
#if NET45_OR_GREATER || NETCOREAPP
    using System.Threading.Tasks;
#endif

    [TestFixture(Category = "CrashReporter.Dump")]
    public class CrashDataTest
    {
        [Test]
        public void PerformCrashDump()
        {
            string fileName = CrashData.Instance.Dump();
            Assert.That(File.Exists(fileName), Is.True);

            Deploy.DeleteDirectory(Path.GetDirectoryName(fileName));
        }

        [Test]
        public void PerformCrashDumpLocalDir()
        {
            using (ScratchPad scratch = Deploy.ScratchPad(ScratchOptions.CreateScratch | ScratchOptions.KeepCurrentDir)) {
                CrashData.Instance.Dump(Path.Combine(scratch.Path, CrashData.Instance.CrashDumpFactory.FileName));

                Assert.That(File.Exists(Path.Combine(scratch.Path, CrashData.Instance.CrashDumpFactory.FileName)), Is.True);
            }
        }

        [Test]
        public void AsynchronousDump()
        {
            // Set up our test factory to allow asynchronous dumps. The implementation may, or may not, do the dump
            // asynchronously.

            ICrashDumpFactory origFactory = CrashData.Instance.CrashDumpFactory;
            MemoryCrashDumpFactory factory = new MemoryCrashDumpFactory {
                IsSynchronous = false
            };
            CrashData.Instance.CrashDumpFactory = factory;

            try {
                string path = CrashData.Instance.Dump();
                Assert.That(path, Is.Not.Null);
                if (Directory.Exists(path)) Deploy.DeleteDirectory(path);
            } finally {
                // The factory is a singleton, so it must be restored for default behaviour with other test cases.
                CrashData.Instance.CrashDumpFactory = origFactory;
            }
        }

#if NET45_OR_GREATER || NETCOREAPP
        [Test]
        public async Task PerformCrashDumpAsync()
        {
            string fileName = await CrashData.Instance.DumpAsync();
            Assert.That(File.Exists(fileName), Is.True);

            Deploy.DeleteDirectory(Path.GetDirectoryName(fileName));
        }

        [Test]
        public async Task PerformCrashDumpLocalDirAsync()
        {
            using (ScratchPad scratch = Deploy.ScratchPad(ScratchOptions.CreateScratch | ScratchOptions.KeepCurrentDir)) {
                await CrashData.Instance.DumpAsync(Path.Combine(scratch.Path, CrashData.Instance.CrashDumpFactory.FileName));

                Assert.That(File.Exists(Path.Combine(scratch.Path, CrashData.Instance.CrashDumpFactory.FileName)), Is.True);
            }
        }

        [Test]
        public async Task AsynchronousDumpAsync()
        {
            // Set up our test factory to allow asynchronous dumps
            ICrashDumpFactory origFactory = CrashData.Instance.CrashDumpFactory;
            MemoryCrashDumpFactory factory = new MemoryCrashDumpFactory {
                IsSynchronous = false
            };
            CrashData.Instance.CrashDumpFactory = factory;

            try {
                string path = await CrashData.Instance.DumpAsync();
                Assert.That(path, Is.Not.Null);
                if (Directory.Exists(path)) Deploy.DeleteDirectory(path);
            } finally {
                // The factory is a singleton, so it must be restored for default behaviour with other test cases.
                CrashData.Instance.CrashDumpFactory = origFactory;
            }
        }
#endif
    }
}
