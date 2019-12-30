namespace RJCP.Diagnostics.CrashData
{
    using System.Collections.Generic;
    using CrashExport;
#if NET45
    using System.Threading.Tasks;
#endif

    public class TestBlock : ICrashDataExport
    {
        private const string TableName = "TestBlock";

        private static readonly Dictionary<string, string> TestBlockData = new Dictionary<string, string>() {
            { "Property", "TestProperty" },
            { "Value", "TestValue" }
        };

        public void Dump(ICrashDataDumpFile dumpFile)
        {
            using (IDumpTable table = dumpFile.DumpTable(TableName, "item")) {
                table.DumpHeader(new string[] { "Property", "Value" });
                table.DumpRow(TestBlockData);
                table.Flush();
            }
        }

#if NET45
        public async Task DumpAsync(ICrashDataDumpFile dumpFile)
        {
            using (IDumpTable table = await dumpFile.DumpTableAsync(TableName, "item")) {
                await table.DumpHeaderAsync(new string[] { "Property", "Value" });
                await table.DumpRowAsync(TestBlockData);
                await table.FlushAsync();
            }
        }
#endif
    }
}
