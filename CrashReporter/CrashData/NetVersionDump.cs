namespace RJCP.Diagnostics.CrashData
{
    using System.Collections.Generic;
    using CrashExport;
#if NET45_OR_GREATER
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// Dump the installed .NET runtimes and the current runtime.
    /// </summary>
    public class NetVersionDump : ICrashDataExport
    {
        private const string NetVersionTable = "NetVersionInstalled";
        private const string NetInstalled = "Installed";
        private const string NetDescription = "Description";

        private const string NetRunningTable = "NetVersionRunning";
        private const string NetRunning = "Running";
        private const string NetRunningVersion = "Version";

        private DumpRow m_RowInstalled = new DumpRow(NetInstalled, NetDescription);
        private DumpRow m_RowRunning = new DumpRow(NetRunning, NetRunningVersion, NetDescription);

        /// <summary>
        /// Dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public void Dump(ICrashDataDumpFile dumpFile)
        {
            IEnumerable<NetVersion.INetVersion> netVersions = new NetVersion.NetVersions();
            IEnumerable<KeyValuePair<string, NetVersion.INetVersion>> runningVersions = GetRunTime();

            using (IDumpTable table = dumpFile.DumpTable(NetVersionTable, "item")) {
                table.DumpHeader(m_RowInstalled);
                foreach (var version in netVersions) {
                    if (version.IsValid) {
                        m_RowInstalled[NetInstalled] = version.Version;
                        m_RowInstalled[NetDescription] = version.Description;
                        table.DumpRow(m_RowInstalled);
                    }
                }
                table.Flush();
            }

            using (IDumpTable table = dumpFile.DumpTable(NetRunningTable, "item")) {
                table.DumpHeader(m_RowRunning);
                foreach (var version in runningVersions) {
                    m_RowRunning[NetRunning] = version.Key;
                    m_RowRunning[NetRunningVersion] = version.Value.Version;
                    m_RowRunning[NetDescription] = version.Value.Description;
                    table.DumpRow(m_RowRunning);
                }
                table.Flush();
            }
        }

#if NET45_OR_GREATER
        /// <summary>
        /// Asynchronously dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        /// <returns>An awaitable task.</returns>
        public async Task DumpAsync(ICrashDataDumpFile dumpFile)
        {
            IEnumerable<NetVersion.INetVersion> netVersions = new NetVersion.NetVersions();
            IEnumerable<KeyValuePair<string, NetVersion.INetVersion>> runningVersions = GetRunTime();

            using (IDumpTable table = await dumpFile.DumpTableAsync(NetVersionTable, "item")) {
                await table.DumpHeaderAsync(m_RowInstalled);
                foreach (var version in netVersions) {
                    if (version.IsValid) {
                        m_RowInstalled[NetInstalled] = version.Version;
                        m_RowInstalled[NetDescription] = version.Description;
                        await table.DumpRowAsync(m_RowInstalled);
                    }
                }
                await table.FlushAsync();
            }

            using (IDumpTable table = await dumpFile.DumpTableAsync(NetRunningTable, "item")) {
                await table.DumpHeaderAsync(m_RowRunning);

                foreach (var version in runningVersions) {
                    m_RowRunning[NetRunning] = version.Key;
                    m_RowRunning[NetRunningVersion] = version.Value.Version;
                    m_RowRunning[NetDescription] = version.Value.Description;
                    await table.DumpRowAsync(m_RowRunning);
                }
                await table.FlushAsync();
            }
        }
#endif

        private IEnumerable<KeyValuePair<string, NetVersion.INetVersion>> GetRunTime()
        {
            List<KeyValuePair<string, NetVersion.INetVersion>> running = new List<KeyValuePair<string, NetVersion.INetVersion>>();

            NetVersion.INetVersion fxRunTime = new NetVersion.Runtime.NetFx();
            if (fxRunTime.IsValid) running.Add(new KeyValuePair<string, NetVersion.INetVersion>("netfx", fxRunTime));
            NetVersion.INetVersion monoRunTime = new NetVersion.Runtime.Mono();
            if (monoRunTime.IsValid) running.Add(new KeyValuePair<string, NetVersion.INetVersion>("mono", monoRunTime));

            return running;
        }
    }
}
