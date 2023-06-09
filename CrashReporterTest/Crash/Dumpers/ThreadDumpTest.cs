namespace RJCP.Diagnostics.Crash.Dumpers
{
    using CrashExport;
    using NUnit.Framework;

    [TestFixture(Category = "CrashReporter.CrashData")]
    public class ThreadDumpTest : DumpTestBase
    {
        protected override ICrashDataExport GetDumper()
        {
            return new ThreadDump();
        }

        protected override string TableName { get { return "OSThreads"; } }
    }
}
