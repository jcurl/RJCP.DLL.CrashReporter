namespace RJCP.Diagnostics.CrashData.NetVersion.NetFx.Net45
{
    using System.Collections.Generic;

    internal static class NetFxConfig
    {
        private static Dictionary<int, NetFxVersion> Versions = new Dictionary<int, NetFxVersion>() {
            { 378389, new NetFxVersion("4.5", ".NET Framework 4.5") },
            { 378675, new NetFxVersion("4.5.1", ".NET Framework 4.5.1 installed with Windows 8.1 or Windows Server 2012 R2") },
            { 378758, new NetFxVersion("4.5.1", ".NET Framework 4.5.1 installed on Windows 8, Windows 7 SP1, or Windows Vista SP2") },
            { 379893, new NetFxVersion("4.5.2", ".NET Framework 4.5.2") },
            { 393295, new NetFxVersion("4.6", ".NET Framework 4.6 on Windows 10 (1507)") },
            { 393297, new NetFxVersion("4.6", ".NET Framework 4.6") },
            { 394254, new NetFxVersion("4.6.1", ".NET Framework 4.6.1 nn Windows 10 November Update systems (1511)") },
            { 394271, new NetFxVersion("4.6.1", ".NET Framework 4.6.1") },
            { 394802, new NetFxVersion("4.6.2", ".NET Framework 4.6.2 on Windows 10 Anniversary Update (1607)") },
            { 394806, new NetFxVersion("4.6.2", ".NET Framework 4.6.2") },
            { 460798, new NetFxVersion("4.7", ".NET Framework 4.7 on Windows 10 Creators Update (1703)") },
            { 460805, new NetFxVersion("4.7", ".NET Framework 4.7") },
            { 461308, new NetFxVersion("4.7.1", ".NET Framework 4.7.1 on Windows 10 Fall Creators Update (1709)") },
            { 461310, new NetFxVersion("4.7.1", ".NET Framework 4.7.1") },
            { 461808, new NetFxVersion("4.7.2", ".NET Framework 4.7.2 on Windows 10 Spring Creators Update (1803)") },
            { 461814, new NetFxVersion("4.7.2", ".NET Framework 4.7.2") },
            { 528040, new NetFxVersion("4.8", ".NET Framework 4.8 on Windows 10 May 2019 Update (1903)") },
            { 528049, new NetFxVersion("4.8", ".NET Framework 4.8") }
        };

        public static NetFxVersion GetNetFxVersion(int release)
        {
            if (Versions.TryGetValue(release, out NetFxVersion version)) return version;
            return null;
        }
    }
}
