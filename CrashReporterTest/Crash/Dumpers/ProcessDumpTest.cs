namespace RJCP.Diagnostics.Crash.Dumpers
{
    using Crash.Export;
    using NUnit.Framework;

    [TestFixture]
    public class ProcessDumpTest : DumpTestBase
    {
        protected override ICrashDataExport GetDumper()
        {
            return new ProcessDumpAccessor();
        }

        protected override string TableName { get { return "ProcessInfo"; } }
    }
}
