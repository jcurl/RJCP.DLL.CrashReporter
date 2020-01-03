namespace RJCP.Diagnostics.CrashData
{
    using CrashExport;
    using NUnit.Framework;
    using OSInfo = NUnit.Framework.OSInfo;

    [TestFixture(Category = "CrashReporter.CrashData")]
    public class WinVerDumpTest : DumpTestBase
    {
        protected override ICrashDataExport GetDumper()
        {
            return new WinVerDump();
        }

        protected override string TableName { get { return "WinOSInfo"; } }

        protected override bool CheckDump(MemoryCrashDataDumpFile dumpFile)
        {
            if (OSInfo.Platform.IsUnix()) return true;
            return base.CheckDump(dumpFile);
        }
    }
}
