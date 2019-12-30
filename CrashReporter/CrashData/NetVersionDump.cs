namespace RJCP.Diagnostics.CrashData
{
    using System.Collections.Generic;
    using CrashExport;
#if NET45
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

        /// <summary>
        /// Dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public void Dump(ICrashDataDumpFile dumpFile)
        {
            IEnumerable<NetVersion.INetVersion> netVersions = new NetVersion.NetVersions();
            IEnumerable<KeyValuePair<string, string>> runningVersions = GetRunTime();

            Dictionary<string, string> installed = new Dictionary<string, string>() {
                { NetInstalled, string.Empty },
                { NetDescription, string.Empty }
            };

            using (IDumpTable table = dumpFile.DumpTable(NetVersionTable, "item")) {
                table.DumpHeader(NetInstalled, NetDescription);

                foreach (var version in netVersions) {
                    if (version.IsValid) {
                        installed[NetInstalled] = version.Version;
                        installed[NetDescription] = version.Description;
                        table.DumpRow(installed);
                    }
                }
                table.Flush();
            }

            Dictionary<string, string> running = new Dictionary<string, string>() {
                { NetRunning, string.Empty },
                { NetDescription, string.Empty }
            };
            using (IDumpTable table = dumpFile.DumpTable(NetRunningTable, "item")) {
                table.DumpHeader(NetRunning, NetDescription);

                foreach (var version in runningVersions) {
                    running[NetRunning] = version.Value;
                    running[NetDescription] = version.Key;
                    table.DumpRow(running);
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
            IEnumerable<NetVersion.INetVersion> netVersions = new NetVersion.NetVersions();
            IEnumerable<KeyValuePair<string, string>> runningVersions = GetRunTime();

            Dictionary<string, string> installed = new Dictionary<string, string>() {
                { NetInstalled, string.Empty },
                { NetDescription, string.Empty }
            };

            using (IDumpTable table = await dumpFile.DumpTableAsync(NetVersionTable, "item")) {
                await table.DumpHeaderAsync(NetInstalled, NetDescription);

                foreach (var version in netVersions) {
                    if (version.IsValid) {
                        installed[NetInstalled] = version.Version;
                        installed[NetDescription] = version.Description;
                        await table.DumpRowAsync(installed);
                    }
                }
                await table.FlushAsync();
            }

            Dictionary<string, string> running = new Dictionary<string, string>() {
                { NetRunning, string.Empty },
                { NetDescription, string.Empty }
            };
            using (IDumpTable table = await dumpFile.DumpTableAsync(NetRunningTable, "item")) {
                await table.DumpHeaderAsync(NetRunning, NetDescription);

                foreach (var version in runningVersions) {
                    running[NetRunning] = version.Value;
                    running[NetDescription] = version.Key;
                    await table.DumpRowAsync(running);
                }
                await table.FlushAsync();
            }
        }
#endif

        private IEnumerable<KeyValuePair<string, string>> GetRunTime()
        {
            List<KeyValuePair<string, string>> running = new List<KeyValuePair<string, string>>();

            NetVersion.INetVersion fxRunTime = new NetVersion.Runtime.NetFx();
            if (fxRunTime.IsValid) running.Add(new KeyValuePair<string, string>("netfx", fxRunTime.Version));
            NetVersion.INetVersion monoRunTime = new NetVersion.Runtime.Mono();
            if (monoRunTime.IsValid) running.Add(new KeyValuePair<string, string>("mono", fxRunTime.Version));

            return running;
        }
    }
}
