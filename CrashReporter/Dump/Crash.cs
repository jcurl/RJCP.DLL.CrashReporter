namespace RJCP.Diagnostics.Dump
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using CrashExport;
#if NET45
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// Singleton objects for creating crash dumps.
    /// </summary>
    public class Crash
    {
        private static Crash s_Instance;
        private static readonly object s_SyncRoot = new object();
        private ICrashDumpFactory m_CrashFactory;
        private IList<ICrashDataExport> m_Providers;

        private Crash() { }

        /// <summary>
        /// Gets an instance to a global Crash object.
        /// </summary>
        /// <value>
        /// An instance to the global crash object.
        /// </value>
        public static Crash Data
        {
            get
            {
                if (s_Instance == null) {
                    lock (s_SyncRoot) {
                        if (s_Instance == null) {
                            s_Instance = new Crash();
                        }
                    }
                }
                return s_Instance;
            }
        }

        /// <summary>
        /// Gets or sets the crash dump factory which implements the file format for the crash file.
        /// </summary>
        /// <value>The crash dump factory.</value>
        public ICrashDumpFactory CrashDumpFactory
        {
            get
            {
                if (m_CrashFactory == null) {
                    lock (s_SyncRoot) {
                        if (m_CrashFactory == null) {
                            m_CrashFactory = new CrashDumpFactory();
                        }
                    }
                }
                return m_CrashFactory;
            }
            set
            {
                lock (s_SyncRoot) {
                    m_CrashFactory = value;
                }
            }
        }

        /// <summary>
        /// Gets a list of providers that is used to collect debug information.
        /// </summary>
        /// <value>A list of providers used to collect debug information.</value>
        /// <remarks>
        /// Add your own provider to this list if you want to collect information that is not provided by this library.
        /// </remarks>
        public IList<ICrashDataExport> Providers
        {
            get
            {
                if (m_Providers == null) {
                    lock (s_SyncRoot) {
                        if (m_Providers == null) {
                            m_Providers = new CrashDataProviders() {
                                new CrashData.NetVersionDump(),
                                new CrashData.AssemblyDump(),
                                new CrashData.EnvironmentDump(),
                                new CrashData.NetworkDump(),
                                new CrashData.ThreadDump(),
                                new CrashData.OSDump(),
                                new CrashData.WinVerDump(),
                                new CrashData.ProcessDump(),
                                new CrashData.ModuleDump()
                            };
                        }
                    }
                }
                return m_Providers;
            }
        }

        /// <summary>
        /// Dumps debugging information to disk, using an automatically generated path.
        /// </summary>
        /// <returns>
        /// Returns a path to a directory or a file where the crash dump was generated.
        /// </returns>
        public string Dump()
        {
            string fileName = GetCrashPath(null);
            return Dump(fileName);
        }

        /// <summary>
        /// Dumps debugging information to disk, writing to the file provided.
        /// </summary>
        /// <param name="fileName">Name of the dump file to generate.</param>
        /// <returns>
        /// Returns a path to a directory or a file where the crash dump was generated.
        /// </returns>
        public string Dump(string fileName)
        {
            using (ICrashDataDumpFile dump = CrashDumpFactory.Create(fileName)) {
                Dump(dump);
            }
            return fileName;
        }

        /// <summary>
        /// Dumps debugging information to a stream, using the directory path for auxiliary files.
        /// </summary>
        /// <param name="stream">The stream to write debug information to.</param>
        /// <param name="path">The path to add debug information to.</param>
        public void Dump(Stream stream, string path)
        {
            using (ICrashDataDumpFile dump = CrashDumpFactory.Create(stream, path)) {
                Dump(dump);
            }
        }

        private void Dump(ICrashDataDumpFile dump)
        {
            foreach (ICrashDataExport dumper in Providers) {
                dumper.Dump(dump);
            }
            dump.Flush();
        }

        internal const string CrashPathRegEx = @"-\d{14}\.[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";

        internal static string GetCrashFolder()
        {
            string basepath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(basepath, "CrashDumps");
        }

        /// <summary>
        /// Generate a full path name for a crash dump folder given a prefix.
        /// </summary>
        /// <param name="prefix">
        /// The prefix to use at the beginning of the crash dump path. If <see langword="null"/>, the current process
        /// name will be used. The directory for the crash dump will be created automatically if it doesn't exist
        /// prior.
        /// </param>
        /// <returns>A fully qualified path that can be given to <see cref="Dump(string)"/>.</returns>
        public string GetCrashPath(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix)) prefix = Process.GetCurrentProcess().ProcessName;
            string name = string.Format("{0}-{1:yyyyMMddHHmmss}.{2}", prefix, DateTime.Now, Guid.NewGuid().ToString());
            string path = Path.Combine(GetCrashFolder(), name);
            if (!Directory.Exists(path)) {
                try {
                    Directory.CreateDirectory(path);
                    return Path.Combine(path, CrashDumpFactory.FileName);
                } catch (PathTooLongException) {  // Creation failed
                } catch (DirectoryNotFoundException) {  // Creation failed
                } catch (NotSupportedException) { // Creation failed
                } catch (IOException) { // Creation failed
                } catch (UnauthorizedAccessException) {  // Creation failed
                }
            }

            return Path.Combine(Environment.CurrentDirectory, name);
        }

#if NET45
        /// <summary>
        /// Dumps debugging information to disk, using an automatically generated path.
        /// </summary>
        /// <returns>An object that can be awaited on.</returns>
        public Task<string> DumpAsync()
        {
            string fileName = GetCrashPath(null);
            return DumpAsync(fileName);
        }

        /// <summary>
        /// Dumps debugging information to disk, writing to the file provided.
        /// </summary>
        /// <param name="fileName">Name of the dump file to generate.</param>
        /// <returns>An object that can be awaited on.</returns>
        public async Task<string> DumpAsync(string fileName)
        {
            using (ICrashDataDumpFile dump = await CrashDumpFactory.CreateAsync(fileName)) {
                await DumpAsync(dump);
            }
            return fileName;
        }

        /// <summary>
        /// Dumps debugging information to a stream, using the directory path for auxiliary files.
        /// </summary>
        /// <param name="stream">The stream to write debug information to.</param>
        /// <param name="path">The path to add debug information to.</param>
        /// <returns>An object that can be awaited on.</returns>
        public async Task DumpAsync(Stream stream, string path)
        {
            using (ICrashDataDumpFile dump = await CrashDumpFactory.CreateAsync(stream, path)) {
                await DumpAsync(dump);
            }
        }

        private async Task DumpAsync(ICrashDataDumpFile dump)
        {
            if (dump.IsSynchronous) {
                foreach (ICrashDataExport dumper in Providers) {
                    await dumper.DumpAsync(dump);
                }
            } else {
                List<Task> dumpers = new List<Task>(Providers.Count);
                foreach (ICrashDataExport dumper in Providers) {
                    dumpers.Add(dumper.DumpAsync(dump));
                }
                await Task.WhenAll(dumpers);
            }
            await dump.FlushAsync();
        }
#endif
    }
}
