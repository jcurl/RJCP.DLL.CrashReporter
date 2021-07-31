namespace RJCP.Diagnostics.CrashData
{
    using System.Collections.Generic;
    using System.Net.NetworkInformation;
    using System.Text;
    using CrashExport;

    /// <summary>
    /// Dump network information to a dump file.
    /// </summary>
    public class NetworkDump : CrashDataExport<NetworkInterface>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkDump"/> class.
        /// </summary>
        public NetworkDump() : base(new DumpRow(AdapterName, AdapterStatus, AdapterDescription, AdapterId,
            AdapterInterfaceType, AdapterSpeed, AdapterMulticastEnabled, AdapterMac,
            AdapterIpDnsSuffix, AdapterIpDnsEnabled, AdapterIpDynDnsEnabled,
            AdapterIpDhcp, AdapterIpUnicast, AdapterIpDns, AdapterIpGateway,
            AdapterIpAnycast, AdapterIpMulticastAddr))
        { }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        protected override string TableName { get { return NetworkTable; } }

        /// <summary>
        /// Gets the name of the row.
        /// </summary>
        /// <value>The name of the row.</value>
        protected override string RowName { get { return "item"; } }

        /// <summary>
        /// An enumerable to get the objects that should be dumped.
        /// </summary>
        /// <returns>An enumerable object.</returns>
        protected override IEnumerable<NetworkInterface> GetRows()
        {
            return NetworkInterface.GetAllNetworkInterfaces();
        }

        /// <summary>
        /// Updates the row given an item.
        /// </summary>
        /// <param name="item">The item returned from <see cref="GetRows()"/>.</param>
        /// <param name="row">The row that should be updated.</param>
        /// <returns>
        /// Returns <see langword="true"/> if the operation was successful and can be added to the dump file, else
        /// <see langword="false"/> that there was a problem and this row should be skipped.
        /// </returns>
        protected override bool UpdateRow(NetworkInterface item, DumpRow row)
        {
            row[AdapterName] = item.Name;
            row[AdapterDescription] = item.Description;
            row[AdapterId] = item.Id;
            row[AdapterInterfaceType] = item.NetworkInterfaceType.ToString();
            row[AdapterStatus] = item.OperationalStatus.ToString();
            row[AdapterSpeed] = item.Speed.ToString();
            row[AdapterMulticastEnabled] = item.SupportsMulticast.ToString();
            row[AdapterMac] = item.GetPhysicalAddress().ToString();

            IPInterfaceProperties properties = item.GetIPProperties();
            row[AdapterIpDnsEnabled] = properties.IsDnsEnabled.ToString();
            row[AdapterIpDynDnsEnabled] = properties.IsDynamicDnsEnabled.ToString();
            row[AdapterIpDnsSuffix] = properties.DnsSuffix;
            row[AdapterIpUnicast] = GetUnicastAddresses(properties.UnicastAddresses);
            row[AdapterIpDns] = GetIpAddresses(properties.DnsAddresses);
            row[AdapterIpGateway] = GetGwAddresses(properties.GatewayAddresses);
            row[AdapterIpAnycast] = GetIpInfoAddresses(properties.AnycastAddresses);
            row[AdapterIpMulticastAddr] = GetMulticastAddresses(properties.MulticastAddresses);
            row[AdapterIpDhcp] = GetIpAddresses(properties.DhcpServerAddresses);

            return true;
        }

        private static string GetUnicastAddresses(UnicastIPAddressInformationCollection list)
        {
            StringBuilder addresses = new StringBuilder();
            foreach (var item in list) {
                if (addresses.Length != 0) addresses.Append("; ");
                addresses.Append(item.Address.AddressFamily.ToString()).Append('/').Append(item.Address.ToString());
            }
            return addresses.ToString();
        }

        private static string GetGwAddresses(GatewayIPAddressInformationCollection list)
        {
            StringBuilder addresses = new StringBuilder();
            foreach (var item in list) {
                if (addresses.Length != 0) addresses.Append("; ");
                addresses.Append(item.Address.AddressFamily.ToString()).Append('/').Append(item.Address.ToString());
            }
            return addresses.ToString();
        }

        private static string GetIpInfoAddresses(IPAddressInformationCollection list)
        {
            StringBuilder addresses = new StringBuilder();
            foreach (var item in list) {
                if (addresses.Length != 0) addresses.Append("; ");
                addresses.Append(item.Address.AddressFamily.ToString()).Append('/').Append(item.Address.ToString());
            }
            return addresses.ToString();
        }

        private static string GetMulticastAddresses(MulticastIPAddressInformationCollection list)
        {
            StringBuilder addresses = new StringBuilder();
            foreach (var item in list) {
                if (addresses.Length != 0) addresses.Append("; ");
                addresses.Append(item.Address.AddressFamily.ToString()).Append('/').Append(item.Address.ToString());
            }
            return addresses.ToString();
        }

        private static string GetIpAddresses(IPAddressCollection list)
        {
            StringBuilder addresses = new StringBuilder();
            foreach (var item in list) {
                if (addresses.Length != 0) addresses.Append("; ");
                addresses.Append(item.AddressFamily.ToString()).Append('/').Append(item.ToString());
            }
            return addresses.ToString();
        }
    }
}
