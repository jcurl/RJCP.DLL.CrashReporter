namespace RJCP.Diagnostics.CrashData
{
    using System;
    using System.Reflection;
    using CrashExport;
    using NUnit.Framework;
#if NET45
    using System.Threading.Tasks;
#endif

    [TestFixture(Category = "CrashReporter.CrashData")]
    public class AssemblyDumpTest
    {
        [Test]
        public void AssemblyDump()
        {
            using (MemoryCrashDataDumpFile dumpFile = new MemoryCrashDataDumpFile()) {
                AssemblyDump dump = new AssemblyDump();
                dump.Dump(dumpFile);
                dumpFile.Flush();

                Assert.That(CheckDump(dumpFile), Is.True);
            }
        }

        private bool CheckDump(MemoryCrashDataDumpFile dumpFile)
        {
            dumpFile.DumpContent();
            Assert.That(dumpFile["Assemblies"].Count, Is.Not.EqualTo(0));

            bool found = false;
            Assembly assemblyDumpAssembly = typeof(AssemblyDump).Assembly;
            foreach (var row in dumpFile["Assemblies"]) {
                if (row["name"].Equals(assemblyDumpAssembly.GetName().Name)) found = true;
            }
            Assert.That(found, Is.True, "Couldn't find assembly '{0}' in dump", assemblyDumpAssembly.GetName().Name);
            return true;
        }

#if NET45
        [Test]
        public async Task AssemblyDumpAsync()
        {
            using (MemoryCrashDataDumpFile dumpFile = new MemoryCrashDataDumpFile()) {
                AssemblyDump dump = new AssemblyDump();
                await dump.DumpAsync(dumpFile);
                await dumpFile.FlushAsync();

                Assert.That(CheckDump(dumpFile), Is.True);
            }
        }
#endif
    }
}
