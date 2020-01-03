namespace RJCP.Diagnostics.CrashData
{
    using System.Collections.Generic;
    using CrashExport;

    /// <summary>
    /// Dump some basic information about the OS.
    /// </summary>
    public class WinVerDump : CrashDataExport<KeyValuePair<string, string>>
    {
        private const string OSInfoTable = "WinOSInfo";
        private const string OSInfoItem = "property";
        private const string OSInfoValue = "value";

        /// <summary>
        /// Initializes a new instance of the <see cref="WinVerDump"/> class.
        /// </summary>
        public WinVerDump() : base(new DumpRow(OSInfoItem, OSInfoValue)) { }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        protected override string TableName { get { return OSInfoTable; } }

        /// <summary>
        /// Gets the name of the row.
        /// </summary>
        /// <value>The name of the row.</value>
        protected override string RowName { get { return "item"; } }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if this instance is valid and data should be dumped; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        protected override bool IsValid() { return Platform.IsWinNT(); }

        /// <summary>
        /// An enumerable to get the objects that should be dumped.
        /// </summary>
        /// <returns>An enumerable object.</returns>
        protected override IEnumerable<KeyValuePair<string, string>> GetRows()
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

        /// <summary>
        /// Updates the row given an item.
        /// </summary>
        /// <param name="item">The item returned from <see cref="GetRows()"/>.</param>
        /// <param name="row">The row that should be updated.</param>
        protected override void UpdateRow(KeyValuePair<string, string> item, DumpRow row)
        {
            row[OSInfoItem] = item.Key;
            row[OSInfoValue] = item.Value;
        }
    }
}
