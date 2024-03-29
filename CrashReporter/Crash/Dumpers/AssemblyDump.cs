﻿namespace RJCP.Diagnostics.Crash.Dumpers
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Versioning;
    using Export;

#if NETCOREAPP || NET45_OR_GREATER
    using System.Linq;
#endif

    /// <summary>
    /// Dumps details of all loaded assemblies in the current domain.
    /// </summary>
    internal sealed class AssemblyDump : CrashDataExport<Assembly>
    {
        private const string AssemblyTable = "Assemblies";
        private const string AssemblyName = "name";
        private const string AssemblyFullName = "fullname";
        private const string AssemblyVersion = "version";
        private const string AssemblyInfoVersion = "versioninfo";
        private const string AssemblyFileVersion = "versionfile";
        private const string AssemblyTarget = "target";
        private const string AssemblyConfig = "config";
        private const string AssemblyLocation = "location";
        private const string AssemblyProcessor = "processor";
#if NETFRAMEWORK
        private const string AssemblyCodeBase = "codebase";
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyDump"/> class.
        /// </summary>
#if NETFRAMEWORK
        public AssemblyDump() : base(new DumpRow(AssemblyName, AssemblyVersion, AssemblyFullName, AssemblyInfoVersion,
            AssemblyFileVersion, AssemblyTarget, AssemblyConfig, AssemblyProcessor, AssemblyLocation, AssemblyCodeBase))
#else
        public AssemblyDump() : base(new DumpRow(AssemblyName, AssemblyVersion, AssemblyFullName, AssemblyInfoVersion,
            AssemblyFileVersion, AssemblyTarget, AssemblyConfig, AssemblyProcessor, AssemblyLocation))
#endif
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
            row[AssemblyTarget] = GetAssemblyTargetFramework(item);
            row[AssemblyConfig] = GetAssemblyConfig(item);
            try {
                row[AssemblyLocation] = item.Location;
#if NET6_0_OR_GREATER
                if (string.IsNullOrEmpty(row[AssemblyLocation]))
                    row[AssemblyLocation] = "(Bundled Assembly)";
#endif
            } catch (NotSupportedException) {
                row[AssemblyLocation] = "(Dynamic Assembly)";
            }
#if NETFRAMEWORK
            try {
                // Raises `NotImplementedException` on .NET 5+ for bundled assemblies.
                //
                // Returns Gets the location of the assembly as specified originally, for example, in an AssemblyName
                // object.
                row[AssemblyCodeBase] = item.CodeBase;
            } catch (NotSupportedException) {
                row[AssemblyCodeBase] = "(Dynamic Assembly)";
            } catch (NotImplementedException) {
                row[AssemblyCodeBase] = "(Unknown)";
            }
#endif
            row[AssemblyProcessor] = GetProcessorArchitecture(item);
            return true;
        }

        private static string GetAssemblyInformationalVersion(Assembly assembly)
        {
            if (Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute))
                is AssemblyInformationalVersionAttribute infoVersion) return infoVersion.InformationalVersion;
            return null;
        }

        private static string GetAssemblyFileVersion(Assembly assembly)
        {
            if (Attribute.GetCustomAttribute(assembly, typeof(AssemblyFileVersionAttribute))
                is AssemblyFileVersionAttribute fileVersion) return fileVersion.Version;
            return null;
        }

        private static string GetAssemblyTargetFramework(Assembly assembly)
        {
            if (Attribute.GetCustomAttribute(assembly, typeof(TargetFrameworkAttribute))
                is TargetFrameworkAttribute target) return target.FrameworkName;
            return null;
        }

        private static string GetAssemblyConfig(Assembly assembly)
        {
            if (Attribute.GetCustomAttribute(assembly, typeof(AssemblyConfigurationAttribute))
                is AssemblyConfigurationAttribute cfg) return cfg.Configuration;
            return null;
        }

        private static string GetProcessorArchitecture(Assembly assembly)
        {
#if NETCOREAPP || NET45_OR_GREATER
            assembly.Modules.First().GetPEKind(out PortableExecutableKinds pekind, out ImageFileMachine machine);
            if (pekind.HasFlag(PortableExecutableKinds.ILOnly))
                return "MSIL";
            switch (machine) {
            case 0: return "<unknown>";
            case ImageFileMachine.I386: return "x86";
            case ImageFileMachine.AMD64: return "x64";
            case ImageFileMachine.ARM: return "ARM";
            case ImageFileMachine.IA64: return "IA64";
            default: return $"{machine}";
            }
#else
            return assembly.GetName().ProcessorArchitecture.ToString();
#endif
        }
    }
}
