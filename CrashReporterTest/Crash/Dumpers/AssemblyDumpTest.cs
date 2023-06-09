namespace RJCP.Diagnostics.Crash.Dumpers
{
    using CrashExport;
    using NUnit.Framework;

    [TestFixture(Category = "CrashReporter.CrashData")]
    public class AssemblyDumpTest : DumpTestBase
    {
        protected override ICrashDataExport GetDumper()
        {
            return new AssemblyDump();
        }

        protected override string TableName { get { return "Assemblies"; } }
    }
}
