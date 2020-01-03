namespace RJCP.Diagnostics.CrashData
{
    using CrashExport;
    using NUnit.Framework;

    [TestFixture(Category = "CrashReporter.CrashData")]
    public class WinVerDumpTest : DumpTestBase
    {
        protected override ICrashDataExport GetDumper()
        {
            return new WinVerDump();
        }

        protected override string TableName { get { return "WinOSInfo"; } }
    }
}
