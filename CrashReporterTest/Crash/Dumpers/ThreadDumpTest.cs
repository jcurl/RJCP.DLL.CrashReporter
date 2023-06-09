namespace RJCP.Diagnostics.Crash.Dumpers
{
    using Crash.Export;
    using NUnit.Framework;

    [TestFixture]
    public class ThreadDumpTest : DumpTestBase
    {
        protected override ICrashDataExport GetDumper()
        {
            return new ThreadDumpAccessor();
        }

        protected override string TableName { get { return "OSThreads"; } }
    }
}
