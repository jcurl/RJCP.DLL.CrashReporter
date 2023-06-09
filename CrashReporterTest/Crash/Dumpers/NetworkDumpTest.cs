namespace RJCP.Diagnostics.Crash.Dumpers
{
    using Crash.Export;
    using NUnit.Framework;

    [TestFixture]
    public class NetworkDumpTest : DumpTestBase
    {
        protected override ICrashDataExport GetDumper()
        {
            return new NetworkDumpAccessor();
        }

        protected override string TableName { get { return "Network"; } }
    }
}
