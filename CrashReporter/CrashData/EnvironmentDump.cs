namespace RJCP.Diagnostics.CrashData
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
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

        /// <summary>
        /// Dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public void Dump(ICrashDataDumpFile dumpFile)
        {
            Dictionary<string, string> variableRow = new Dictionary<string, string>() {
                { EnvName, string.Empty },
                { EnvValue, string.Empty }
            };
            using (IDumpTable table = dumpFile.DumpTable(EnvTable, "item")) {
                table.DumpHeader(EnvName, EnvValue);

                foreach (DictionaryEntry variable in Environment.GetEnvironmentVariables()) {
                    variableRow[EnvName] = variable.Key.ToString();
                    variableRow[EnvValue] = variable.Value.ToString();
                    table.DumpRow(variableRow);
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
            Dictionary<string, string> variableRow = new Dictionary<string, string>() {
                { EnvName, string.Empty },
                { EnvValue, string.Empty }
            };
            using (IDumpTable table = await dumpFile.DumpTableAsync(EnvTable, "item")) {
                await table.DumpHeaderAsync(EnvName, EnvValue);

                foreach (DictionaryEntry variable in Environment.GetEnvironmentVariables()) {
                    variableRow[EnvName] = variable.Key.ToString();
                    variableRow[EnvValue] = variable.Value.ToString();
                    await table.DumpRowAsync(variableRow);
                }
                await table.FlushAsync();
            }
        }
#endif
    }
}
