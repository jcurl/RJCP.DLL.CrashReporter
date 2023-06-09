namespace RJCP.Diagnostics.Crash.Dumpers
{
    using CrashExport;
    using NUnit.Framework;

    [TestFixture(Category = "CrashReporter.CrashData")]
    public class NetworkDumpTest : DumpTestBase
    {
        protected override ICrashDataExport GetDumper()
        {
            return new NetworkDump();
        }

        protected override string TableName { get { return "Network"; } }
    }
}
