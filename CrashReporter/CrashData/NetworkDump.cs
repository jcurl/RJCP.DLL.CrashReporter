namespace RJCP.Diagnostics.CrashData
{
    using System.Net.NetworkInformation;
    using System.Text;
    using CrashExport;
#if NET45
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// Dump network information to a dump file.
    /// </summary>
    public class NetworkDump : ICrashDataExport
    {
        private const string NetworkTable = "Network";
        private const string AdapterName = "name";
        private const string AdapterDescription = "description";
        private const string AdapterId = "id";
        private const string AdapterInterfaceType = "type";
        private const string AdapterStatus = "status";
        private const string AdapterSpeed = "speed";
        private const string AdapterMulticastEnabled = "multicast";
        private const string AdapterMac = "mac";
        private const string AdapterIpDnsSuffix = "dnssuffix";
        private const string AdapterIpDnsEnabled = "dnsenabled";
        private const string AdapterIpDynDnsEnabled = "dnsdynenabled";
        private const string AdapterIpDhcp = "dhcpaddr";
        private const string AdapterIpUnicast = "ipaddr";
        private const string AdapterIpDns = "dnsaddr";
        private const string AdapterIpGateway = "gwaddr";
        private const string AdapterIpAnycast = "anycastaddr";
        private const string AdapterIpMulticastAddr = "multicastaddr";

        private DumpRow m_Row = new DumpRow(
            AdapterName, AdapterStatus, AdapterDescription, AdapterId,
            AdapterInterfaceType, AdapterSpeed, AdapterMulticastEnabled, AdapterMac,
            AdapterIpDnsSuffix, AdapterIpDnsEnabled, AdapterIpDynDnsEnabled,
            AdapterIpDhcp, AdapterIpUnicast, AdapterIpDns, AdapterIpGateway,
            AdapterIpAnycast, AdapterIpMulticastAddr);

        /// <summary>
        /// Dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public void Dump(ICrashDataDumpFile dumpFile)
        {
            using (IDumpTable table = dumpFile.DumpTable(NetworkTable, "item")) {
                table.DumpHeader(m_Row);

                NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in adapters) {
                    GetRow(adapter, m_Row);
                    table.DumpRow(m_Row);
                }
                table.Flush();
            }
        }

        private void GetRow(NetworkInterface adapter, DumpRow row)
        {
            row[AdapterName] = adapter.Name;
            row[AdapterDescription] = adapter.Description;
            row[AdapterId] = adapter.Id;
            row[AdapterInterfaceType] = adapter.NetworkInterfaceType.ToString();
            row[AdapterStatus] = adapter.OperationalStatus.ToString();
            row[AdapterSpeed] = adapter.Speed.ToString();
            row[AdapterMulticastEnabled] = adapter.SupportsMulticast.ToString();
            row[AdapterMac] = adapter.GetPhysicalAddress().ToString();

            IPInterfaceProperties properties = adapter.GetIPProperties();
            row[AdapterIpDnsEnabled] = properties.IsDnsEnabled.ToString();
            row[AdapterIpDynDnsEnabled] = properties.IsDynamicDnsEnabled.ToString();
            row[AdapterIpDnsSuffix] = properties.DnsSuffix;
            row[AdapterIpUnicast] = GetUnicastAddresses(properties.UnicastAddresses);
            row[AdapterIpDns] = GetIpAddresses(properties.DnsAddresses);
            row[AdapterIpGateway] = GetGwAddresses(properties.GatewayAddresses);
            row[AdapterIpAnycast] = GetIpInfoAddresses(properties.AnycastAddresses);
            row[AdapterIpMulticastAddr] = GetMulticastAddresses(properties.MulticastAddresses);
            row[AdapterIpDhcp] = GetIpAddresses(properties.DhcpServerAddresses);
        }

        private string GetUnicastAddresses(UnicastIPAddressInformationCollection list)
        {
            StringBuilder addresses = new StringBuilder();
            foreach (var item in list) {
                if (addresses.Length != 0) addresses.Append("; ");
                addresses.Append(item.Address.AddressFamily.ToString()).Append("/").Append(item.Address.ToString());
            }
            return addresses.ToString();
        }

        private string GetGwAddresses(GatewayIPAddressInformationCollection list)
        {
            StringBuilder addresses = new StringBuilder();
            foreach (var item in list) {
                if (addresses.Length != 0) addresses.Append("; ");
                addresses.Append(item.Address.AddressFamily.ToString()).Append("/").Append(item.Address.ToString());
            }
            return addresses.ToString();
        }

        private string GetIpInfoAddresses(IPAddressInformationCollection list)
        {
            StringBuilder addresses = new StringBuilder();
            foreach (var item in list) {
                if (addresses.Length != 0) addresses.Append("; ");
                addresses.Append(item.Address.AddressFamily.ToString()).Append("/").Append(item.Address.ToString());
            }
            return addresses.ToString();
        }

        private string GetMulticastAddresses(MulticastIPAddressInformationCollection list)
        {
            StringBuilder addresses = new StringBuilder();
            foreach (var item in list) {
                if (addresses.Length != 0) addresses.Append("; ");
                addresses.Append(item.Address.AddressFamily.ToString()).Append("/").Append(item.Address.ToString());
            }
            return addresses.ToString();
        }

        private string GetIpAddresses(IPAddressCollection list)
        {
            StringBuilder addresses = new StringBuilder();
            foreach (var item in list) {
                if (addresses.Length != 0) addresses.Append("; ");
                addresses.Append(item.AddressFamily.ToString()).Append("/").Append(item.ToString());
            }
            return addresses.ToString();
        }

#if NET45
        /// <summary>
        /// Asynchronously dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public async Task DumpAsync(ICrashDataDumpFile dumpFile)
        {
            using (IDumpTable table = await dumpFile.DumpTableAsync(NetworkTable, "item")) {
                await table.DumpHeaderAsync(m_Row);

                NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in adapters) {
                    GetRow(adapter, m_Row);
                    await table.DumpRowAsync(m_Row);
                }
                await table.FlushAsync();
            }
        }
#endif
    }
}
