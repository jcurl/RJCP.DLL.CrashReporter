namespace RJCP.Diagnostics.Crash.Dumpers
{
    using System.Collections.Generic;
    using Crash.Export;
#if !NET40_LEGACY
    using System.Threading.Tasks;
#endif

    public class TestBlock : ICrashDataExport
    {
        private const string TableName = "TestBlock";

        private readonly Dictionary<string, string> m_TestBlockData = new() {
            { "Property", "TestProperty" },
            { "Value", "TestValue" }
        };

        public void AddEntry(string key, string value)
        {
            m_TestBlockData.Add(key, value);
        }

        public void Dump(ICrashDataDumpFile dumpFile)
        {
            using (IDumpTable table = dumpFile.DumpTable(TableName, "item")) {
                table.DumpHeader(m_TestBlockData.Keys);
                table.DumpRow(m_TestBlockData);
                table.Flush();
            }
        }

#if !NET40_LEGACY
        public async Task DumpAsync(ICrashDataDumpFile dumpFile)
        {
            using (IDumpTable table = await dumpFile.DumpTableAsync(TableName, "item")) {
                await table.DumpHeaderAsync(m_TestBlockData.Keys);
                await table.DumpRowAsync(m_TestBlockData);
                await table.FlushAsync();
            }
        }
#endif
    }
}
