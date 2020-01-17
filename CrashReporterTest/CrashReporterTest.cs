namespace RJCP.Diagnostics
{
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions",
            Justification = "Test case should run to end without crashing.")]
        public void CleanDumpFolder()
        {
            CrashReporter.CleanUpDump();
        }
    }
}
