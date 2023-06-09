namespace RJCP.Diagnostics.Crash.Dumpers
{
    using Crash.Export;
    using NUnit.Framework;
    using RJCP.Core.Environment;

    [TestFixture]
    public class WinVerDumpTest : DumpTestBase
    {
        protected override ICrashDataExport GetDumper()
        {
            return new WinVerDumpAccessor();
        }

        protected override string TableName { get { return "WinOSInfo"; } }

        protected override bool CheckDump(MemoryCrashDataDumpFile dumpFile)
        {
            if (Platform.IsUnix()) return true;
            return base.CheckDump(dumpFile);
        }
    }
}
