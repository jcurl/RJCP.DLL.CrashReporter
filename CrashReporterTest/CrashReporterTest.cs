namespace RJCP.Diagnostics
{
    using System.Diagnostics;
    using NUnit.Framework;

    [TestFixture(Category = "CrashReporter")]
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
            string dump = CrashReporter.CreateDump(Dump.CoreType.None);
            Assert.That(dump, Is.Not.Null.Or.Empty);
        }

        [TestCase]
        public void DoDumpChangedPrefix()
        {
            string prefix = string.Format("{0}.test", Process.GetCurrentProcess().ProcessName);
            string fileName = Dump.Crash.Data.GetCrashPath(prefix);
            Assert.That(fileName, Is.Not.Null.Or.Empty);

            string dump = CrashReporter.CreateDump(fileName, Dump.CoreType.None);
            Assert.That(dump, Is.Not.Null.Or.Empty);
        }

        [TestCase]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions",
            Justification = "Test case should run to end without crashing.")]
        public void CleanDumpFolder()
        {
            CrashReporter.CleanUpDump();
        }
    }
}
