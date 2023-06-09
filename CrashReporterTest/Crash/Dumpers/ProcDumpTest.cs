namespace RJCP.Diagnostics.Crash.Dumpers
{
    using CrashExport;
    using NUnit.Framework;

    [TestFixture(Category = "CrashReporter.CrashData")]
    public class ProcDumpTest : DumpTestBase
    {
        protected override ICrashDataExport GetDumper()
        {
            return new ProcessDump();
        }

        protected override string TableName { get { return "ProcessInfo"; } }
    }
}
