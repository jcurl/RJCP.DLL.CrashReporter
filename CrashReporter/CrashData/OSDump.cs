namespace RJCP.Diagnostics.CrashData
{
    using System;
    using System.Collections.Generic;
    using CrashExport;
#if NET45
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// Dump some basic information about the OS.
    /// </summary>
    public class OSDump : ICrashDataExport
    {
        private const string OSInfoTable = "OSInfo";
        private const string OSInfoItem = "property";
        private const string OSInfoValue = "value";

        private DumpRow m_Row = new DumpRow(OSInfoItem, OSInfoValue);

        /// <summary>
        /// Dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public void Dump(ICrashDataDumpFile dumpFile)
        {
            using (IDumpTable table = dumpFile.DumpTable(OSInfoTable, "item")) {
                table.DumpHeader(m_Row);
                foreach (var item in GetOsInfo()) {
                    m_Row[OSInfoItem] = item.Key;
                    m_Row[OSInfoValue] = item.Value;
                    table.DumpRow(m_Row);
                }
                table.Flush();
            }
        }

        private IList<KeyValuePair<string, string>> GetOsInfo()
        {
            return new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("version", Environment.OSVersion.VersionString),
                new KeyValuePair<string, string>("platform", Environment.OSVersion.Platform.ToString()),
                new KeyValuePair<string, string>("servicepack", Environment.OSVersion.ServicePack),
                new KeyValuePair<string, string>("os64bit", Environment.Is64BitOperatingSystem.ToString()),
                new KeyValuePair<string, string>("proc64bit", Environment.Is64BitProcess.ToString()),
                new KeyValuePair<string, string>("hostname", Environment.MachineName),
                new KeyValuePair<string, string>("domain", Environment.UserDomainName),
                new KeyValuePair<string, string>("username", Environment.UserName),
            };
        }

#if NET45
        /// <summary>
        /// Asynchronously dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public async Task DumpAsync(ICrashDataDumpFile dumpFile)
        {
            using (IDumpTable table = await dumpFile.DumpTableAsync(OSInfoTable, "item")) {
                await table.DumpHeaderAsync(m_Row);
                foreach (var item in GetOsInfo()) {
                    m_Row[OSInfoItem] = item.Key;
                    m_Row[OSInfoValue] = item.Value;
                    await table.DumpRowAsync(m_Row);
                }
                await table.FlushAsync();
            }
        }
#endif
    }
}
