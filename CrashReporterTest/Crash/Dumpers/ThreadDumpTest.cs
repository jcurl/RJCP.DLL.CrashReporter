namespace RJCP.Diagnostics.Crash.Dumpers
{
    using Crash.Export;
    using NUnit.Framework;

    [TestFixture(Category = "CrashReporter.CrashData")]
    public class ThreadDumpTest : DumpTestBase
    {
        protected override ICrashDataExport GetDumper()
        {
            return new ThreadDumpAccessor();
        }

        protected override string TableName { get { return "OSThreads"; } }
    }
}
