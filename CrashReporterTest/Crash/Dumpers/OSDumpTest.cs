namespace RJCP.Diagnostics.Crash.Dumpers
{
    using Crash.Export;
    using NUnit.Framework;

    [TestFixture]
    public class OSDumpTest : DumpTestBase
    {
        protected override ICrashDataExport GetDumper()
        {
            return new OSDumpAccessor();
        }

        protected override string TableName { get { return "OSInfo"; } }
    }
}
