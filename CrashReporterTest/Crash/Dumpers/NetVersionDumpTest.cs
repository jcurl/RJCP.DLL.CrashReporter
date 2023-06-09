namespace RJCP.Diagnostics.Crash.Dumpers
{
    using CrashExport;
    using NUnit.Framework;

    [TestFixture(Category = "CrashReporter.CrashData")]
    public class NetVersionDumpTest : DumpTestBase
    {
        protected override ICrashDataExport GetDumper()
        {
            return new NetVersionDump();
        }

        protected override string TableName { get { return "NetVersionInstalled"; } }
    }
}
