﻿namespace RJCP.Diagnostics.Crash.Dumpers
{
    using Crash.Export;
    using NUnit.Framework;

    [TestFixture]
    public class AssemblyDumpTest : DumpTestBase
    {
        protected override ICrashDataExport GetDumper()
        {
            return new AssemblyDumpAccessor();
        }

        protected override string TableName { get { return "Assemblies"; } }
    }
}
