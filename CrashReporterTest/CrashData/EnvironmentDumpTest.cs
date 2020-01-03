namespace RJCP.Diagnostics.CrashData
{
    using CrashExport;
    using NUnit.Framework;

    [TestFixture(Category = "CrashReporter.CrashData")]
    public class EnvironmentDumpTest : DumpTestBase
    {
        protected override ICrashDataExport GetDumper()
        {
            return new EnvironmentDump();
        }

        protected override string TableName { get { return "EnvironmentVariables"; } }
    }
}
