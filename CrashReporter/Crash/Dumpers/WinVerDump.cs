namespace RJCP.Diagnostics.Crash.Dumpers
{
    using System.Collections.Generic;
    using System.Runtime.Versioning;
    using Export;
    using RJCP.Core.Environment;

    /// <summary>
    /// Dump some basic information about the OS.
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal sealed class WinVerDump : CrashDataExport<KeyValuePair<string, string>>
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
            OSVersion.OSVersion winVer = new();
            return new List<KeyValuePair<string, string>>() {
                new("version", winVer.Version.ToString()),
                new("servicepack", winVer.ServicePack.ToString()),
                new("csdversion", winVer.CsdVersion),
                new("platform", winVer.PlatformId.ToString()),
                new("productInfo", winVer.ProductInfo.ToString()),
                new("productType", winVer.ProductType.ToString()),
                new("suite", winVer.SuiteFlags.ToString()),
                new("nativearchitecture", winVer.NativeArchitecture),
                new("architecture", winVer.Architecture),
                new("r2", winVer.ServerR2.ToString()),
                new("releaseInfo", winVer.ReleaseInfo)
            };
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
        protected override bool UpdateRow(KeyValuePair<string, string> item, DumpRow row)
        {
            row[OSInfoItem] = item.Key;
            row[OSInfoValue] = item.Value;
            return true;
        }
    }
}
