namespace RJCP.Diagnostics.CrashData
{
    using System;
    using System.Collections;
    using CrashExport;
#if NET45
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// Dump all environment variables for the current process.
    /// </summary>
    public class EnvironmentDump : ICrashDataExport
    {
        private const string EnvTable = "EnvironmentVariables";
        private const string EnvName = "name";
        private const string EnvValue = "value";

        private DumpRow m_Row = new DumpRow(EnvName, EnvValue);

        /// <summary>
        /// Dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public void Dump(ICrashDataDumpFile dumpFile)
        {
            using (IDumpTable table = dumpFile.DumpTable(EnvTable, "item")) {
                table.DumpHeader(m_Row);
                foreach (DictionaryEntry variable in Environment.GetEnvironmentVariables()) {
                    m_Row[EnvName] = variable.Key.ToString();
                    m_Row[EnvValue] = variable.Value.ToString();
                    table.DumpRow(m_Row);
                }
                table.Flush();
            }
        }

#if NET45
        /// <summary>
        /// Asynchronously dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public async Task DumpAsync(ICrashDataDumpFile dumpFile)
        {
            using (IDumpTable table = await dumpFile.DumpTableAsync(EnvTable, "item")) {
                await table.DumpHeaderAsync(m_Row);
                foreach (DictionaryEntry variable in Environment.GetEnvironmentVariables()) {
                    m_Row[EnvName] = variable.Key.ToString();
                    m_Row[EnvValue] = variable.Value.ToString();
                    await table.DumpRowAsync(m_Row);
                }
                await table.FlushAsync();
            }
        }
#endif
    }
}
