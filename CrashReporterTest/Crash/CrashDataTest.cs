﻿namespace RJCP.Diagnostics.Crash
{
    using System.IO;
    using Export;
    using NUnit.Framework;
    using RJCP.CodeQuality.NUnitExtensions;
#if !NET40_LEGACY
    using System.Threading.Tasks;
#endif

    [TestFixture]
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
            ICrashDumpFactory origFactory = CrashData.Instance.CrashDumpFactory;
            CrashData.Instance.CrashDumpFactory = new MemoryCrashDumpFactory();

            try {
                string path = CrashData.Instance.Dump();
                Assert.That(path, Is.Not.Null);
                if (Directory.Exists(path)) Deploy.DeleteDirectory(path);
            } finally {
                // The factory is a singleton, so it must be restored for default behaviour with other test cases.
                CrashData.Instance.CrashDumpFactory = origFactory;
            }
        }

#if !NET40_LEGACY
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
            CrashData.Instance.CrashDumpFactory = new MemoryCrashDumpFactory();

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
