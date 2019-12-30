namespace RJCP.Diagnostics.CrashData
{
    using System;
    using System.Reflection;
    using CrashExport;
#if NET45
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// Dumps details of all loaded assemblies in the current domain.
    /// </summary>
    /// <seealso cref="RJCP.Diagnostics.CrashExport.ICrashDataExport" />
    public class AssemblyDump : ICrashDataExport
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

        private DumpRow m_Row = new DumpRow(
            AssemblyName, AssemblyVersion, AssemblyFullName, AssemblyInfoVersion,
            AssemblyFileVersion, AssemblyProcessor, AssemblyLocation, AssemblyCodeBase);

        /// <summary>
        /// Dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public void Dump(ICrashDataDumpFile dumpFile)
        {
            using (IDumpTable table = dumpFile.DumpTable(AssemblyTable, "assembly")) {
                table.DumpHeader(m_Row);

                AppDomain domain = AppDomain.CurrentDomain;
                foreach (Assembly assembly in domain.GetAssemblies()) {
                    GetAssemblyInformation(assembly, m_Row);
                    table.DumpRow(m_Row);
                }
                table.Flush();
            }
        }

        private void GetAssemblyInformation(Assembly assembly, DumpRow assemblyRow)
        {
            assemblyRow[AssemblyName] = assembly.GetName().Name;
            assemblyRow[AssemblyFullName] = assembly.FullName;
            assemblyRow[AssemblyVersion] = assembly.GetName().Version.ToString();
            assemblyRow[AssemblyInfoVersion] = GetAssemblyInformationalVersion(assembly);
            assemblyRow[AssemblyFileVersion] = GetAssemblyFileVersion(assembly);
            assemblyRow[AssemblyLocation] = assembly.Location;
            assemblyRow[AssemblyCodeBase] = assembly.CodeBase;
            assemblyRow[AssemblyProcessor] = assembly.GetName().ProcessorArchitecture.ToString();
        }

        private string GetAssemblyInformationalVersion(Assembly assembly)
        {
            if (!(Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute))
                is AssemblyInformationalVersionAttribute infoVersion)) return null;
            return infoVersion.InformationalVersion;
        }

        private string GetAssemblyFileVersion(Assembly assembly)
        {
            if (!(Attribute.GetCustomAttribute(assembly, typeof(AssemblyFileVersionAttribute))
                is AssemblyFileVersionAttribute fileVersion)) return null;
            return fileVersion.Version;
        }

#if NET45
        /// <summary>
        /// Asynchronously dumps debug information using the provided dump interface.
        /// </summary>
        /// <param name="dumpFile">The dump interface to write properties to.</param>
        public async Task DumpAsync(ICrashDataDumpFile dumpFile)
        {
            using (IDumpTable table = await dumpFile.DumpTableAsync(AssemblyTable, "assembly")) {
                await table.DumpHeaderAsync(m_Row);

                AppDomain domain = AppDomain.CurrentDomain;
                foreach (Assembly assembly in domain.GetAssemblies()) {
                    GetAssemblyInformation(assembly, m_Row);
                    await table.DumpRowAsync(m_Row);
                }
                await table.FlushAsync();
            }
        }
#endif
    }
}
