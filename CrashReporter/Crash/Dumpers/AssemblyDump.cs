namespace RJCP.Diagnostics.Crash.Dumpers
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Export;

    /// <summary>
    /// Dumps details of all loaded assemblies in the current domain.
    /// </summary>
    public class AssemblyDump : CrashDataExport<Assembly>
    {
        private const string AssemblyTable = "Assemblies";
        private const string AssemblyName = "name";
        private const string AssemblyFullName = "fullname";
        private const string AssemblyVersion = "version";
        private const string AssemblyInfoVersion = "versioninfo";
        private const string AssemblyFileVersion = "versionfile";
        private const string AssemblyLocation = "location";
        private const string AssemblyCodeBase = "codebase";
        private const string AssemblyProcessor = "processor";

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyDump"/> class.
        /// </summary>
        public AssemblyDump() : base(new DumpRow(AssemblyName, AssemblyVersion, AssemblyFullName, AssemblyInfoVersion,
            AssemblyFileVersion, AssemblyProcessor, AssemblyLocation, AssemblyCodeBase))
        { }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        protected override string TableName { get { return AssemblyTable; } }

        /// <summary>
        /// Gets the name of the row.
        /// </summary>
        /// <value>The name of the row.</value>
        protected override string RowName { get { return "assembly"; } }

        /// <summary>
        /// An enumerable to get the objects that should be dumped.
        /// </summary>
        /// <returns>An enumerable object.</returns>
        protected override IEnumerable<Assembly> GetRows()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
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
        protected override bool UpdateRow(Assembly item, DumpRow row)
        {
            row[AssemblyName] = item.GetName().Name;
            row[AssemblyFullName] = item.FullName;
            row[AssemblyVersion] = item.GetName().Version.ToString();
            row[AssemblyInfoVersion] = GetAssemblyInformationalVersion(item);
            row[AssemblyFileVersion] = GetAssemblyFileVersion(item);
            try {
                row[AssemblyLocation] = item.Location;
            } catch (NotSupportedException) {
                row[AssemblyLocation] = "(Dynamic Assembly)";
            }
            try {
                row[AssemblyCodeBase] = item.CodeBase;
            } catch (NotSupportedException) {
                row[AssemblyCodeBase] = "(Dynamic Assembly)";
            }
            row[AssemblyProcessor] = item.GetName().ProcessorArchitecture.ToString();
            return true;
        }

        private static string GetAssemblyInformationalVersion(Assembly assembly)
        {
            if (!(Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute))
                is AssemblyInformationalVersionAttribute infoVersion)) return null;
            return infoVersion.InformationalVersion;
        }

        private static string GetAssemblyFileVersion(Assembly assembly)
        {
            if (!(Attribute.GetCustomAttribute(assembly, typeof(AssemblyFileVersionAttribute))
                is AssemblyFileVersionAttribute fileVersion)) return null;
            return fileVersion.Version;
        }
    }
}
