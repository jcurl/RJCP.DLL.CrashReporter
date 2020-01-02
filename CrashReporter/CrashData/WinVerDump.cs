namespace RJCP.Diagnostics.CrashData
{
    using System.Collections.Generic;
    using CrashExport;
#if NET45
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// Dump some basic information about the OS.
    /// </summary>
    public class WinVerDump : ICrashDataExport
    {
        private const string OSInfoTable = "WinOSInfo";
        private const string OSInfoItem = "property";
        private const string OSInfoValue = "value";

        private DumpRow m_Row = new DumpRow(OSInfoItem, OSInfoValue);

        /// <summary>
        /// Dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public void Dump(ICrashDataDumpFile dumpFile)
        {
            if (!Platform.IsWinNT()) return;
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
            OSVersion.OSVersion winVer = new OSVersion.OSVersion();
            return new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("version", winVer.Version.ToString()),
                new KeyValuePair<string, string>("servicepack", winVer.ServicePack.ToString()),
                new KeyValuePair<string, string>("csdversion", winVer.CsdVersion),
                new KeyValuePair<string, string>("platform", winVer.PlatformId.ToString()),
                new KeyValuePair<string, string>("productInfo", winVer.ProductInfo.ToString()),
                new KeyValuePair<string, string>("productType", winVer.ProductType.ToString()),
                new KeyValuePair<string, string>("suite", winVer.SuiteFlags.ToString()),
                new KeyValuePair<string, string>("architecture", winVer.Architecture.ToString()),
                new KeyValuePair<string, string>("r2", winVer.ServerR2.ToString()),
            };
        }

#if NET45
        /// <summary>
        /// Asynchronously dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public async Task DumpAsync(ICrashDataDumpFile dumpFile)
        {
            if (!Platform.IsWinNT()) return;
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
