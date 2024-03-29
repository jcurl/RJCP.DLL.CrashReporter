﻿namespace RJCP.Diagnostics.Crash.Dumpers
{
    using Crash.Export;
    using NUnit.Framework;

    [TestFixture]
    public class ModuleDumpTest : DumpTestBase
    {
        protected override ICrashDataExport GetDumper()
        {
            return new ModuleDumpAccessor();
        }

        protected override string TableName { get { return "Modules"; } }
    }
}
