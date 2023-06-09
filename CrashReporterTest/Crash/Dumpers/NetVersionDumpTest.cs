namespace RJCP.Diagnostics.Crash.Dumpers
{
    using Crash.Export;
    using NUnit.Framework;

    [TestFixture]
    public class NetVersionDumpTest : DumpTestBase
    {
        protected override ICrashDataExport GetDumper()
        {
            return new NetVersionDumpAccessor();
        }

        protected override string TableName { get { return "NetVersionInstalled"; } }
    }
}
