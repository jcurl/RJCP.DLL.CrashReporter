﻿namespace RJCP.Diagnostics
{
    using System.Diagnostics;
    using System.IO;
    using Crash;
    using NUnit.Framework;
    using RJCP.CodeQuality.NUnitExtensions;

    [TestFixture]
    public class CrashReporterTest
    {
        [TestCase]
        public void DoDump()
        {
            string dump = CrashReporter.CreateDump();
            Assert.That(dump, Is.Not.Null.Or.Empty);
        }

        [TestCase]
        public void DoDumpNoCore()
        {
            string dump = CrashReporter.CreateDump(CoreType.None);
            Assert.That(dump, Is.Not.Null.Or.Empty);
        }

        [TestCase]
        public void DoDumpChangedPrefix()
        {
            using (Process proc = Process.GetCurrentProcess()) {
                string prefix = string.Format("{0}.test", proc.ProcessName);
                string path = CrashData.GetCrashDir(prefix);
                Assert.That(path, Is.Not.Null.Or.Empty);
                string dump = CrashReporter.CreateDump(path, CoreType.None);
                Assert.That(File.Exists(dump), Is.True);
            }
        }

        [TestCase]
        public void CreateDumpRelativeFileName()
        {
            using (ScratchPad scratch = Deploy.ScratchPad(ScratchOptions.UseDeployDir)) {
                string path = Path.Combine(scratch.RelativePath, "dump");
                string dump = CrashReporter.CreateDump(path, CoreType.None);
                Assert.That(File.Exists(dump), Is.True);
            }
        }

        [TestCase]
        public void CreateDumpAbsoluteFileName()
        {
            using (ScratchPad scratch = Deploy.ScratchPad(ScratchOptions.UseDeployDir)) {
                string path = Path.Combine(scratch.Path, "dump");
                string dump = CrashReporter.CreateDump(path, CoreType.None);
                Assert.That(File.Exists(dump), Is.True);
            }
        }

        [TestCase]
        public void CreateDumpRelativeFileNameZipExt()
        {
            using (ScratchPad scratch = Deploy.ScratchPad(ScratchOptions.UseDeployDir)) {
                string path = Path.Combine(scratch.RelativePath, "dump.zip");
                string dump = CrashReporter.CreateDump(path, CoreType.None);
                Assert.That(File.Exists(dump), Is.True);
                Assert.That(Path.GetFileNameWithoutExtension(dump), Is.EqualTo("dump"));
            }
        }

        [TestCase]
        public void CreateDumpAbsoluteFileNameZipExt()
        {
            using (ScratchPad scratch = Deploy.ScratchPad(ScratchOptions.UseDeployDir)) {
                string path = Path.Combine(scratch.Path, "dump.zip");
                string dump = CrashReporter.CreateDump(path, CoreType.None);
                Assert.That(File.Exists(dump), Is.True);
                Assert.That(Path.GetFileNameWithoutExtension(dump), Is.EqualTo("dump"));
            }
        }

        [TestCase]
        public void CleanDumpFolder()
        {
            CrashReporter.CleanUpDump();
        }
    }
}
