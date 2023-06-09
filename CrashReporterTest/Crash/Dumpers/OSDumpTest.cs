namespace RJCP.Diagnostics.Crash.Dumpers
{
    using CrashExport;
    using NUnit.Framework;

    [TestFixture(Category = "CrashReporter.CrashData")]
    public class OSDumpTest : DumpTestBase
    {
        protected override ICrashDataExport GetDumper()
        {
            return new OSDump();
        }

        protected override string TableName { get { return "OSInfo"; } }
    }
}
