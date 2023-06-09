namespace RJCP.Diagnostics.Crash.Dumpers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Export;

    /// <summary>
    /// Dump all environment variables for the current process.
    /// </summary>
    public class EnvironmentDump : CrashDataExport<KeyValuePair<string, string>>
    {
        private const string EnvTable = "EnvironmentVariables";
        private const string EnvName = "name";
        private const string EnvValue = "value";

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentDump"/> class.
        /// </summary>
        public EnvironmentDump() : base(new DumpRow(EnvName, EnvValue)) { }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        protected override string TableName { get { return EnvTable; } }

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
            foreach (DictionaryEntry variable in Environment.GetEnvironmentVariables()) {
                yield return new KeyValuePair<string, string>(variable.Key.ToString(), variable.Value.ToString());
            }
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
            row[EnvName] = item.Key;
            row[EnvValue] = item.Value;
            return true;
        }
    }
}
