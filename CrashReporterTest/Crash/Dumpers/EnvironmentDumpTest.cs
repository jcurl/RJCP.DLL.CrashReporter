namespace RJCP.Diagnostics.Crash.Dumpers
{
    using Crash.Export;
    using NUnit.Framework;

    [TestFixture]
    public class EnvironmentDumpTest : DumpTestBase
    {
        protected override ICrashDataExport GetDumper()
        {
            return new EnvironmentDumpAccessor();
        }

        protected override string TableName { get { return "EnvironmentVariables"; } }
    }
}
