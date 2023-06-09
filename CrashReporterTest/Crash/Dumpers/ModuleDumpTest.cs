namespace RJCP.Diagnostics.Crash.Dumpers
{
    using Crash.Export;
    using NUnit.Framework;

    [TestFixture(Category = "CrashReporter.CrashData")]
    public class ModuleDumpTest : DumpTestBase
    {
        protected override ICrashDataExport GetDumper()
        {
            return new ModuleDump();
        }

        protected override string TableName { get { return "Modules"; } }
    }
}
