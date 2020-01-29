namespace RJCP.Diagnostics.CrashData
{
    using System;
    using System.Collections.Generic;
    using CrashExport;

    /// <summary>
    /// Dump some basic information about the OS.
    /// </summary>
    public class OSDump : CrashDataExport<KeyValuePair<string, string>>
    {
        private const string OSInfoTable = "OSInfo";
        private const string OSInfoItem = "property";
        private const string OSInfoValue = "value";

        /// <summary>
        /// Initializes a new instance of the <see cref="OSDump" /> class.
        /// </summary>
        public OSDump() : base(new DumpRow(OSInfoItem, OSInfoValue)) { }

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
        /// An enumerable to get the objects that should be dumped.
        /// </summary>
        /// <returns>An enumerable object.</returns>
        protected override IEnumerable<KeyValuePair<string, string>> GetRows()
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
