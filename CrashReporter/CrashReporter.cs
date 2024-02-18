namespace RJCP.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.ExceptionServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Crash;
    using Crash.Archive;

    /// <summary>
    /// Crash Reporter static class for logging crash information.
    /// </summary>
    /// <remarks>
    /// Use this class to register handlers, that when an unhandled exception occurs, you receive context information
    /// and a core dump for later debugging.
    /// </remarks>
    public static class CrashReporter
    {
        /// <summary>
        /// Specifies what kind of core dump to create.
        /// </summary>
        /// <value>The type of core dump to generate.</value>
        public static CoreType CoreType { get; set; } = CoreType.MiniDump;

        /// <summary>
        /// Gets the watchdog for thread monitoring.
        /// </summary>
        /// <value>The watchdog for thread monitoring.</value>
        public static Watchdog.IWatchdog Watchdog { get; } = new Watchdog.ThreadWatchdog();

        /// <summary>
        /// Gets the CrashReporter configuration.
        /// </summary>
        /// <value>The CrashReporter configuration.</value>
        public static Config.CrashReporter.CrashConfig Config { get; } = new Config.CrashReporter.CrashConfig();

        /// <summary>
        /// Sets the exception handlers for crash reporting.
        /// </summary>
        /// <example>
        /// <code language="csharp"><![CDATA[
        /// Log.MyTraceSource = new TraceSource("MyApp.Trace");
        /// CrashReporter.Source = Log.MyTraceSource;
        /// CrashReporter.SetExceptionHandlers();
        /// ]]></code>
        /// </example>
        public static void SetExceptionHandlers()
        {
            SetFirstChanceException();
            SetUnhandledException();
        }

        /// <summary>
        /// Sets the unhandled exception.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if it was set, <see langword="false"/> otherwise.</returns>
        /// <example>
        /// <code language="csharp"><![CDATA[
        /// Log.MyTraceSource = new TraceSource("MyApp.Trace");
        /// CrashReporter.Source = Log.MyTraceSource;
        /// CrashReporter.SetUnhandledException();
        /// ]]></code>
        /// </example>
        public static bool SetUnhandledException()
        {
            AppDomain appDomain = AppDomain.CurrentDomain;
            appDomain.UnhandledException += UnhandledExceptionHandler;
            return true;
        }

        /// <summary>
        /// Sets the first chance exception.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if it was set, <see langword="false"/> otherwise.</returns>
        /// <remarks>
        /// Mono does not implement the event for setting the first chance exception, so when compiling on Mono,
        /// compilation fails. This method abstracts that and uses reflection to determine if the method exists, and
        /// if it was set. Thus, using this method allows for writing platform portable code between the .NET
        /// framework and Mono.
        /// <para>
        /// This method sets the first chance exception handler for the current application domain. Note, there is no
        /// guarantee that this method will be called. It was tested to work on Windows .NET Framework, but tested not
        /// to work on Mono under Linux.
        /// </para>
        /// <para>The handler is set to the handler provided by this library.</para>
        /// </remarks>
        /// <example>
        /// <code language="csharp"><![CDATA[
        /// Log.MyTraceSource = new TraceSource("MyApp.Trace");
        /// CrashReporter.Source = Log.MyTraceSource;
        /// CrashReporter.SetFirstChanceException();
        /// ]]></code>
        /// </example>
        public static bool SetFirstChanceException()
        {
            AppDomain appDomain = AppDomain.CurrentDomain;
            Native.AppDomainAccessor accessor = new(appDomain);
            if (!accessor.IsSupported) return false;
            return accessor.SetFirstChanceException(FirstChanceExceptionHandler);
        }

        /// <summary>
        /// Sets the first chance exception.
        /// </summary>
        /// <param name="handler">The handler that should be called.</param>
        /// <returns>Returns <see langword="true"/> if it was set, <see langword="false"/> otherwise.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="handler"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// Mono does not implement the event for setting the first chance exception, so when compiling on Mono,
        /// compilation fails. This method abstracts that and uses reflection to determine if the method exists, and
        /// if it was set. Thus, using this method allows for writing platform portable code between the .NET
        /// framework and Mono.
        /// <para>
        /// This method sets the first chance exception handler for the current application domain. Note, there is no
        /// guarantee that this method will be called. It was tested to work on Windows .NET Framework, but tested not
        /// to work on Mono under Linux.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code language="csharp"><![CDATA[
        /// Log.MyTraceSource = new TraceSource("MyApp.Trace");
        /// CrashReporter.Source = Log.MyTraceSource;
        /// CrashReporter.SetExceptionHandlers(CrashReporter.FirstChanceExceptionHandler);
        /// ]]></code>
        /// </example>
        public static bool SetFirstChanceException(EventHandler<FirstChanceExceptionEventArgs> handler)
        {
            AppDomain appDomain = AppDomain.CurrentDomain;
            return SetFirstChanceException(appDomain, handler);
        }

        /// <summary>
        /// Sets the first chance exception.
        /// </summary>
        /// <param name="appDomain">
        /// The application domain. You can use this as an extension method for an <see cref="AppDomain"/>.
        /// </param>
        /// <param name="handler">The handler that should be called.</param>
        /// <returns>Returns <see langword="true"/> if it was set, <see langword="false"/> otherwise.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="handler"/> is <see langword="null"/>.
        /// <para>- or -</para>
        /// <paramref name="appDomain"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// Mono does not implement the event for setting the first chance exception, so when compiling on Mono,
        /// compilation fails. This method abstracts that and uses reflection to determine if the method exists, and
        /// if it was set. Thus, using this method allows for writing platform portable code between the .NET
        /// framework and Mono.
        /// <para>
        /// This method sets the first chance exception handler for the current application domain. Note, there is no
        /// guarantee that this method will be called. It was tested to work on Windows .NET Framework, but tested not
        /// to work on Mono under Linux.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code language="csharp"><![CDATA[
        /// Log.MyTraceSource = new TraceSource("MyApp.Trace");
        /// CrashReporter.Source = Log.MyTraceSource;
        /// AppDomain.CurrentDomain.SetFirstChanceException(CrashReporter.FirstChanceExceptionHandler);
        /// ]]></code>
        /// </example>
        public static bool SetFirstChanceException(this AppDomain appDomain, EventHandler<FirstChanceExceptionEventArgs> handler)
        {
            ThrowHelper.ThrowIfNull(appDomain);
            ThrowHelper.ThrowIfNull(handler);
            return SetFirstChanceExceptionUser(appDomain, handler);
        }

        private static bool SetFirstChanceExceptionUser(AppDomain appDomain, EventHandler<FirstChanceExceptionEventArgs> handler)
        {
            Native.AppDomainAccessor accessor = new(appDomain);
            if (accessor.IsSupported) {
                UserHandler += handler;
                return accessor.SetFirstChanceException(FirstChanceExceptionHandlerUser);
            }
            return false;
        }

        private static event EventHandler<FirstChanceExceptionEventArgs> UserHandler;

        /// <summary>
        /// Handles the first chance exception handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">
        /// The <see cref="FirstChanceExceptionEventArgs"/> instance containing the event data.
        /// </param>
        /// <remarks>
        /// Assign the <see cref="AppDomain.FirstChanceException"/> handler to this method for automatically handling
        /// exceptions. This won't result in a crash dump, but will provide logging.
        /// <para>
        /// Not all platforms provide this method, so use the
        /// <see cref="SetFirstChanceException(EventHandler{FirstChanceExceptionEventArgs})"/> to assign to this
        /// method.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code language="csharp"><![CDATA[
        /// Log.MyTraceSource = new TraceSource("MyApp.Trace");
        /// CrashReporter.Source = Log.MyTraceSource;
        /// CrashReporter.SetFirstChanceException(CrashReporter.FirstChanceExceptionHandler);
        /// ]]></code>
        /// </example>
        public static void FirstChanceExceptionHandler(object sender, FirstChanceExceptionEventArgs args)
        {
            int counter = Thread.VolatileRead(ref SuppressFirstChanceExceptionCounter);
            if (counter > 0) return;

            if (Log.CrashLog.ShouldTrace(TraceEventType.Information)) {
                StackTrace stack = new(true);
                Log.CrashLog.TraceEvent(TraceEventType.Information,
                    "First Chance Exception ({0}): {1}\n{2}",
                    args.Exception.GetType().ToString(), args.Exception.Message, stack.ToString());
            }
        }

        private static void FirstChanceExceptionHandlerUser(object sender, FirstChanceExceptionEventArgs args)
        {
            int counter = Thread.VolatileRead(ref SuppressFirstChanceExceptionCounter);
            if (counter > 0) return;

            EventHandler<FirstChanceExceptionEventArgs> handler = UserHandler;
            if (handler is not null) handler(sender, args);
        }

        /// <summary>
        /// Handles an Unhandled Exception.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">
        /// The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.
        /// </param>
        /// <remarks>
        /// Assign the <see cref="AppDomain.UnhandledException"/> handler to this method for automatically handling
        /// crash dumps for the user.
        /// </remarks>
        /// <example>
        /// <code language="csharp"><![CDATA[
        /// Log.MyTraceSource = new TraceSource("MyApp.Trace");
        /// CrashReporter.Source = Log.MyTraceSource;
        /// AppDomain.CurrentDomain.UnhandledException += CrashReporter.UnhandledExceptionHandler;
        /// ]]></code>
        /// </example>
        public static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            if (Log.CrashLog.ShouldTrace(TraceEventType.Critical)) {
                if (args.IsTerminating) {
                    Log.CrashLog.TraceEvent(TraceEventType.Critical, "Unhandled Exception: Terminating!");
                }
                StackTrace stack = new(true);
                if (args.ExceptionObject is not Exception ex) {
                    Log.CrashLog.TraceEvent(TraceEventType.Critical,
                        "Unhandled Exception ({0}): {1}\n{2}",
                        args.ExceptionObject.GetType().ToString(), args.ExceptionObject.ToString(), stack.ToString());
                } else {
                    Log.CrashLog.TraceEvent(TraceEventType.Critical,
                        "Unhandled Exception ({0}): {1}\n{2}",
                        ex.GetType().ToString(), ex.Message, stack.ToString());
                }
            }

            try {
                _ = CreateDump();
            } catch {
                Log.CrashLog.TraceEvent(TraceEventType.Error, "Crash dump failed");
            }
        }

        /// <summary>
        /// Creates a dump with a mini dump according to <see cref="CoreType"/>.
        /// </summary>
        /// <returns>The name of the file that was created.</returns>
        /// <remarks>
        /// You can use this method to generate your own core dump as required, for example, if an exception is caught
        /// (so that the <see cref="AppDomain.UnhandledException"/> won't be raised).
        /// </remarks>
        public static string CreateDump()
        {
            return CreateDump(null, CoreType);
        }

        /// <summary>
        /// Creates a dump with a mini dump according to <see cref="CoreType"/>.
        /// </summary>
        /// <param name="fileName">
        /// File name of the compressed crash dump file to create without the extension (the extension will be
        /// automatically added based on the type of compression used).
        /// </param>
        /// <returns>The name of the file that was created.</returns>
        /// <remarks>
        /// You can use this method to generate your own core dump as required, for example, if an exception is caught
        /// (so that the <see cref="AppDomain.UnhandledException"/> won't be raised).
        /// </remarks>
        public static string CreateDump(string fileName)
        {
            return CreateDump(fileName, CoreType);
        }

        /// <summary>
        /// Creates a dump with the minidump specified.
        /// </summary>
        /// <param name="coreType">Type of the core dump to create.</param>
        /// <returns>The name of the file that was generated, so the user might be notified.</returns>
        /// <remarks>
        /// You can use this method to generate your own core dump as required, for example, if an exception is caught
        /// (so that the <see cref="AppDomain.UnhandledException"/> won't be raised).
        /// </remarks>
        public static string CreateDump(CoreType coreType)
        {
            return CreateDump(null, coreType);
        }

        /// <summary>
        /// Creates a dump with the minidump specified.
        /// </summary>
        /// <param name="fileName">
        /// File name of the compressed crash dump file to create without the extension (the extension will be
        /// automatically added based on the type of compression used).
        /// </param>
        /// <param name="coreType">Type of the core dump to create.</param>
        /// <returns>The name of the file that was generated, so the user might be notified.</returns>
        /// <remarks>
        /// You can use this method to generate a core dump programmatically with crash data. It is useful during
        /// unexpected exceptions.
        /// <para>
        /// The <paramref name="fileName"/> is the name of the compressed dump file to create. It should not have an
        /// extension, as the correct extension based on the compression scheme is automatically added. If it contains
        /// a recognized extension, that will be used.
        /// </para>
        /// <para>
        /// A temporary directory of the same name as <paramref name="fileName"/> without the extension, is created
        /// (or used if it already exists) to prepare the compressed file. This allows the directory to be first
        /// created and data to be populated which should be provided in addition to crash dump. In this case,
        /// <paramref name="fileName"/> is the directory which should be used for creating the compressed dump file.
        /// </para>
        /// </remarks>
        public static string CreateDump(string fileName, CoreType coreType)
        {
            if (fileName is null) {
                try {
                    CleanUpDump();
                } catch { /* Ignore any errors while trying to clean up the dump, so we can continue to crash */ }
            }

            string crashDumpFile = null;
            if (fileName is not null) {
                // If the file name has a known extension, then remove it. It will be added back later.
                string extension = Path.GetExtension(fileName);
                if (extension.Equals(".zip", StringComparison.OrdinalIgnoreCase)) {
                    fileName = Path.Combine(
                        Path.GetDirectoryName(fileName),
                        Path.GetFileNameWithoutExtension(fileName));
                }
                crashDumpFile = Path.Combine(fileName, CrashData.Instance.CrashDumpFactory.FileName);
            }

            // At any time, the contents of the crash may be deleted, perhaps by another process also using this
            // assembly. Must capture all errors, and assume that when creating the core, data is removed,
            // probably because `CleanUpDump` is being called from another process.

            string crashDumpPath;
            try {
                if (crashDumpFile is null) {
                    crashDumpPath = CrashData.Instance.Dump();
                } else {
                    crashDumpPath = CrashData.Instance.Dump(crashDumpFile);
                }
                if (crashDumpPath is null) {
                    Log.CrashLog.TraceEvent(TraceEventType.Warning, "Crash Data not created");
                    return null;
                }
            } catch (Exception ex) {
                Log.CrashLog.TraceEvent(TraceEventType.Error, "Error creating dump: {0}", ex.ToString());
                throw;
            }

            string coreDumpDir;
            try {
                if (File.Exists(crashDumpPath)) {
                    // This is a file, not a directory, so we get the directory portion and put the core dump next to
                    // it.
                    coreDumpDir = Path.GetDirectoryName(crashDumpPath);
                } else if (Directory.Exists(crashDumpPath)) {
                    coreDumpDir = crashDumpPath;
                } else {
                    Log.CrashLog.TraceEvent(TraceEventType.Warning, "Crash Data was removed: {0}", crashDumpPath ?? "(empty)");
                    return null;
                }

                string coreDumpName = string.Format("{0}.{1}.dmp",
                    ProcessInfo.ProcessName, ProcessInfo.ProcessId);
                string coreDumpPath = Path.Combine(coreDumpDir, coreDumpName);
                Core.Create(coreDumpPath, coreType);
            } catch (Exception ex) {
                Log.CrashLog.TraceEvent(TraceEventType.Error, "Error creating core: {0}", ex.ToString());
                throw;
            }

            string dumpFileName;
            try {
                dumpFileName = Compress.CompressFolder(coreDumpDir);
                if (dumpFileName is not null && File.Exists(dumpFileName)) {
                    // Only delete if compression was successful.
                    Log.CrashLog.TraceEvent(TraceEventType.Warning, $"CreateDump DeleteFolder {coreDumpDir}");
                    FileSystem.DeleteFolder(coreDumpDir);

                    if (Directory.Exists(coreDumpDir))
                        Log.CrashLog.TraceEvent(TraceEventType.Warning, "Couldn't complete remove folder {0}", coreDumpDir);
                }
            } catch (Exception ex) {
                Log.CrashLog.TraceEvent(TraceEventType.Error, "Compressing folder Exception: {0}", ex.ToString());
                throw;
            }

            Log.CrashLog.TraceEvent(TraceEventType.Warning, "Crash dump created at: {0}", dumpFileName);
            return dumpFileName;
        }

        private const long GbMultiplier = 1024 * 1024 * 1024;

        /// <summary>
        /// Removes old dump files from the default dump directory created when using <see cref="CreateDump()"/>.
        /// </summary>
        /// <remarks>
        /// Over time, the number of files in the dump may take a significant amount of space. This method allows to
        /// programmatically remove old dumps and large dumps as disk space is reduced.
        /// <para>
        /// A log might be required to be deleted, but if there is a file system error, it will be skipped.
        /// </para>
        /// </remarks>
        public static void CleanUpDump()
        {
            string dumpFolder = CrashData.GetCrashFolder();
            CleanUpDump(dumpFolder, CrashData.CrashPathRegEx);
        }

        /// <summary>
        /// Removes old dump files from the default dump directory created when using <see cref="CreateDump()"/>.
        /// </summary>
        /// <param name="dumpFolder">The dump folder to clean.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dumpFolder"/> is <see langword="null"/>.
        /// </exception>
        public static void CleanUpDump(string dumpFolder)
        {
            CleanUpDump(dumpFolder, null);
        }

        /// <summary>
        /// Removes old dump files from the default dump directory created when using <see cref="CreateDump()"/>.
        /// </summary>
        /// <param name="dumpFolder">The dump folder to clean.</param>
        /// <param name="fileMatchRegEx">The file match regular expression to only erase dumps that match.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dumpFolder"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// Over time, the number of files in the dump may take a significant amount of space. This method allows to
        /// programmatically remove old dumps and large dumps as disk space is reduced.
        /// <para>
        /// A log might be required to be deleted, but if there is a file system error, it will be skipped.
        /// </para>
        /// </remarks>
        public static void CleanUpDump(string dumpFolder, string fileMatchRegEx)
        {
            ThrowHelper.ThrowIfNull(dumpFolder);
            if (!Directory.Exists(dumpFolder)) return;

            Regex crashFileRegex = null;
            if (fileMatchRegEx is not null) {
                try {
                    crashFileRegex = new Regex(fileMatchRegEx);
                } catch (ArgumentException) {
                    // Ignore invalid regex. We don't clean.
                    return;
                }
            }
            IList<FileSystemInfo> crashCandidates = new List<FileSystemInfo>();

            DirectoryInfo directory;
            try {
                directory = new DirectoryInfo(dumpFolder);
            } catch (PathTooLongException) {
                return;
            }
            DriveInfo drive = new(directory.Root.FullName);

            DirectoryInfo[] subDirs = directory.GetDirectories();
            if (subDirs is not null) {
                foreach (DirectoryInfo subDir in subDirs) {
                    if (crashFileRegex is null || crashFileRegex.IsMatch(subDir.Name)) {
                        crashCandidates.Add(subDir);
                    }
                }
            }

            FileInfo[] files = directory.GetFiles();
            if (files is not null) {
                foreach (FileInfo file in files) {
                    if (crashFileRegex is null || crashFileRegex.IsMatch(file.Name)) {
                        crashCandidates.Add(file);
                    }
                }
            }

            crashCandidates = CleanUpDumpOld(Config.CrashDumper.DumpDir.AgeDays, crashCandidates);
            crashCandidates = CleanUpDumpKeepNewest(Config.CrashDumper.DumpDir.MaxLogs, crashCandidates);
            _ = CleanUpKeepSpace(
                drive,
                Config.CrashDumper.DumpDir.ReserveFree * GbMultiplier, Config.CrashDumper.DumpDir.ReserveFreePercent,
                Config.CrashDumper.DumpDir.MaxDirSize * GbMultiplier, Config.CrashDumper.DumpDir.MaxDirSizeMinLogs,
                crashCandidates);
        }

        private static IList<FileSystemInfo> CleanUpDumpOld(int ageDays, IList<FileSystemInfo> candidates)
        {
            if (candidates.Count == 0) return candidates;

            // Remove all logs/directories that are older than 45 days old
            IList<FileSystemInfo> remaining = new List<FileSystemInfo>();
            DateTime now = DateTime.UtcNow;
            foreach (var candidate in candidates) {
                if (now.Subtract(candidate.CreationTimeUtc).TotalDays > ageDays) {
                    FileSystem.Delete(candidate);
                } else {
                    remaining.Add(candidate);
                }
            }
            return remaining;
        }

        private static IList<FileSystemInfo> CleanUpDumpKeepNewest(int count, IList<FileSystemInfo> candidates)
        {
            if (candidates.Count == 0) return candidates;

            IList<FileSystemInfo> remaining = new List<FileSystemInfo>();
            int candidateCount = candidates.Count;
            var created = from candidate in candidates orderby candidate.CreationTime select candidate;
            DateTime now = DateTime.UtcNow;
            foreach (var candidate in created) {
                // Keep all dumps that are less than 5 minutes old, so the user can still capture all relevant data.
                if (now.Subtract(candidate.CreationTimeUtc).TotalMinutes > 5) {
                    if (candidateCount > count) {
                        FileSystem.Delete(candidate);
                        --candidateCount;
                    } else {
                        remaining.Add(candidate);
                    }
                }
            }
            return remaining;
        }

        /// <summary>
        /// Removes candidates from disk based on the amount of space required and disk free.
        /// </summary>
        /// <param name="drive">The drive to use for the amount of free space.</param>
        /// <param name="minDiskFree">The minimum disk free space required in bytes.</param>
        /// <param name="minPercent">The minimum percent of the disk total size as a value from 0 to 100.</param>
        /// <param name="maxUsage">The maximum number of bytes allowed.</param>
        /// <param name="minFiles">The minimum files that can be kept.</param>
        /// <param name="candidates">The file candidates that should be checked and deleted.</param>
        /// <returns>The list of files remaining on disk (those not removed).</returns>
        /// <remarks>
        /// Files are removed based on their size. Files are removed if:
        /// <list>
        /// <item>
        /// The amount of disk space required exceeds the <paramref name="minDiskFree"/> or the
        /// <paramref name="minPercent"/> (whichever is higher), so that there should be a "reserve" number of bytes
        /// of disk space, regardless of how much space is required.
        /// </item>
        /// <item>
        /// THe amount of space the logs require exceeds the <paramref name="maxUsage"/>, regardless of the amount of
        /// disk space that is free. This puts an upper limit on the amount of logs in use always. But we're allowed
        /// to keep <paramref name="minFiles"/> and exceed the <paramref name="maxUsage"/> in this case.
        /// </item>
        /// </list>
        /// Logs are always deleted in the order of the oldest first (based on the creation time).
        /// </remarks>
        private static IList<FileSystemInfo> CleanUpKeepSpace(DriveInfo drive, long minDiskFree, int minPercent, long maxUsage, int minFiles, IList<FileSystemInfo> candidates)
        {
            if (candidates.Count == 0) return candidates;

            Dictionary<FileSystemInfo, long> sizes = new();
            long totalSize = 0;
            foreach (var candidate in candidates) {
                long size = FileSystem.GetSize(candidate);
                sizes[candidate] = size;
                totalSize += size;
            }

            // Don't need to delete files if the total size is under the maxUsage and there is enough disk reserve.
            long diskReserve = Math.Max(minDiskFree, drive.TotalSize * minPercent / 100);
            if (totalSize < maxUsage && drive.AvailableFreeSpace > diskReserve) return candidates;

            // Remove from oldest to newest first if:
            // * the reserve is exceeded; or
            // * if max usage exceeded and there are more than minFiles
            IList<FileSystemInfo> remaining = new List<FileSystemInfo>();
            int candidateCount = candidates.Count;
            var created = from candidate in candidates orderby candidate.CreationTime select candidate;
            foreach (var candidate in created) {
                if (drive.AvailableFreeSpace < diskReserve ||
                    candidateCount > minFiles && totalSize >= maxUsage) {
                    FileSystem.Delete(candidate);
                    totalSize -= sizes[candidate];
                    --candidateCount;
                } else {
                    remaining.Add(candidate);
                }
            }
            return remaining;
        }

        private static int SuppressFirstChanceExceptionCounter;

        private sealed class SuppressFirstChanceExceptionBlock : IDisposable
        {
            public SuppressFirstChanceExceptionBlock()
            {
                Interlocked.Increment(ref SuppressFirstChanceExceptionCounter);
            }

            public void Dispose()
            {
                Interlocked.Decrement(ref SuppressFirstChanceExceptionCounter);
            }
        }

        /// <summary>
        /// Suppresses the first chance exception, until the object returned is disposed.
        /// </summary>
        /// <returns>A reference object that when disposed, enables printing first chance exceptions.</returns>
        public static IDisposable SuppressFirstChanceException()
        {
            return new SuppressFirstChanceExceptionBlock();
        }
    }
}
