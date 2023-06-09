namespace RJCP.Diagnostics.Crash.Dumpers
{
    using CrashExport;
    using NUnit.Framework;
    using RJCP.Core.Environment;

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
            if (Platform.IsUnix()) return true;
            return base.CheckDump(dumpFile);
        }
    }
}
