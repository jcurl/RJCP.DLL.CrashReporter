namespace RJCP.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.ExceptionServices;
    using System.Text.RegularExpressions;
    using Dump;

    /// <summary>
    /// Crash Reporter static class for logging crash information.
    /// </summary>
    public static class CrashReporter
    {
        /// <summary>
        /// Gets or sets the source which should be used for crash handling logging.
        /// </summary>
        /// <value>The source that should be used for logging in case of a crash.</value>
        /// <example>
        /// <code lang="csharp">
        /// Log.MyTraceSource = new TraceSource("MyApp.Trace");
        /// CrashReporter.Source = Log.MyTraceSource;
        /// CrashReporter.SetExceptionHandlers();
        /// </code>
        /// </example>
        public static TraceSource Source { get; set; }

        /// <summary>
        /// Specifies what kind of core dump to create.
        /// </summary>
        /// <value>The type of core dump to generate.</value>
        public static CoreType CoreType { get; set; } = CoreType.MiniDump;

        /// <summary>
        /// Sets the exception handlers for crash reporting.
        /// </summary>
        /// <example>
        /// <code lang="csharp">
        /// Log.MyTraceSource = new TraceSource("MyApp.Trace");
        /// CrashReporter.Source = Log.MyTraceSource;
        /// CrashReporter.SetExceptionHandlers();
        /// </code>
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
        /// <code lang="csharp">
        /// Log.MyTraceSource = new TraceSource("MyApp.Trace");
        /// CrashReporter.Source = Log.MyTraceSource;
        /// CrashReporter.SetUnhandledException();
        /// </code>
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
        /// <code lang="csharp">
        /// Log.MyTraceSource = new TraceSource("MyApp.Trace");
        /// CrashReporter.Source = Log.MyTraceSource;
        /// CrashReporter.SetFirstChanceException();
        /// </code>
        /// </example>
        public static bool SetFirstChanceException()
        {
            AppDomain appDomain = AppDomain.CurrentDomain;
            return SetFirstChanceException(appDomain, FirstChanceExceptionHandler);
        }

        /// <summary>
        /// Sets the first chance exception.
        /// </summary>
        /// <param name="handler">The handler that should be called.</param>
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
        /// </remarks>
        /// <example>
        /// <code lang="csharp">
        /// Log.MyTraceSource = new TraceSource("MyApp.Trace");
        /// CrashReporter.Source = Log.MyTraceSource;
        /// CrashReporter.SetExceptionHandlers(CrashReporter.FirstChanceExceptionHandler);
        /// </code>
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
        /// <code lang="csharp">
        /// Log.MyTraceSource = new TraceSource("MyApp.Trace");
        /// CrashReporter.Source = Log.MyTraceSource;
        /// CrashReporter.SetFirstChanceException(AppDomain.CurrentDomain, CrashReporter.FirstChanceExceptionHandler);
        /// </code>
        /// </example>
        public static bool SetFirstChanceException(this AppDomain appDomain, EventHandler<FirstChanceExceptionEventArgs> handler)
        {
            Native.AppDomainAccessor accessor = new Native.AppDomainAccessor(appDomain);
            return accessor.SetFirstChanceException(handler);
        }

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
        /// <code lang="csharp">
        /// Log.MyTraceSource = new TraceSource("MyApp.Trace");
        /// CrashReporter.Source = Log.MyTraceSource;
        /// CrashReporter.SetFirstChanceException(CrashReporter.FirstChanceExceptionHandler);
        /// </code>
        /// </example>
        public static void FirstChanceExceptionHandler(object sender, FirstChanceExceptionEventArgs args)
        {
            if (Source == null) return;
            if (Source.Switch.ShouldTrace(TraceEventType.Information)) {
                StackTrace stack = new StackTrace(true);
                Source.TraceEvent(TraceEventType.Information, 0,
                    "First Chance Exception: {0}\nFirst Chance Exception: Stack Trace:\n{1}",
                    args.Exception.Message, stack.ToString());
            }
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
        /// <code lang="csharp">
        /// Log.MyTraceSource = new TraceSource("MyApp.Trace");
        /// CrashReporter.Source = Log.MyTraceSource;
        /// AppDomain.CurrentDomain.UnhandledException += CrashReporter.UnhandledExceptionHandler;
        /// </code>
        /// </example>
        public static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            if (Source != null && Source.Switch.ShouldTrace(TraceEventType.Critical)) {
                if (args.IsTerminating) {
                    Source.TraceEvent(TraceEventType.Critical, 0, "Unhandled Exception: Terminating!");
                }
                StackTrace stack = new StackTrace(true);
                if (!(args.ExceptionObject is Exception ex)) {
                    Source.TraceEvent(TraceEventType.Critical, 0,
                        "Unhandled Exception: {0}\nUnhandled Exception: Stack Trace:\n{1}",
                        args.ExceptionObject.ToString(), stack.ToString());
                } else {
                    Source.TraceEvent(TraceEventType.Critical, 0,
                        "Unhandled Exception: {0}\nUnhandled Exception: Stack Trace:\n{1}",
                        ex.Message, stack.ToString());
                }
            }

            try {
                string path = CreateDump();
                Log.CrashLog.TraceEvent(TraceEventType.Information, 0, "Crash dump created at: {0}", path);
            } catch {
                Log.CrashLog.TraceEvent(TraceEventType.Error, 0, "Crash dump failed");
            }
        }

        /// <summary>
        /// Creates a dump with a mini dump according to <see cref="CoreType"/>.
        /// </summary>
        /// <returns>The name of the file that was created</returns>
        /// <remarks>
        /// You can use this method to generate your own core dump as required, for example, if an exception is caught
        /// (so that the <see cref="AppDomain.UnhandledException"/> won't be raised).
        /// </remarks>
        public static string CreateDump()
        {
            return CreateDump(CoreType);
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
            try {
                CleanUpDump();
            } catch { /* Ignore any errors while trying to clean up the dump, so we can continue to crash */ }

            string path;
            try {
                path = Crash.Data.Dump();
                if (path == null) return null;
            } catch (Exception ex) {
                Log.CrashLog.TraceEvent(TraceEventType.Error, 0, "Error creating dump: {0}", ex.ToString());
                throw;
            }

            string dumpDir;
            try {
                if (File.Exists(path)) {
                    // This is a file, not a directory, so we get the directory portion.
                    dumpDir = Path.GetDirectoryName(path);
                } else if (Directory.Exists(path)) {
                    dumpDir = path;
                } else {
                    return null;
                }

                string dumpName = string.Format("{0}.{1}.dmp", Process.GetCurrentProcess().ProcessName, Process.GetCurrentProcess().Id);
                string coreName = Path.Combine(dumpDir, dumpName);
                Core.MiniDump(coreName, coreType);
            } catch (Exception ex) {
                Log.CrashLog.TraceEvent(TraceEventType.Error, 0, "Error creating core: {0}", ex.ToString());
                throw;
            }

            string dumpFileName;
            try {
                dumpFileName = Dump.Archive.Compress.CompressFolder(dumpDir);
                if (dumpFileName != null && File.Exists(dumpFileName)) {
                    // Only delete if compression was successful.
                    Dump.Archive.FileSystem.DeleteFolder(dumpDir);

                    if (Directory.Exists(dumpDir))
                        Log.CrashLog.TraceEvent(TraceEventType.Warning, 0, "Couldn't complete remove folder {0}", dumpDir);
                }
            } catch (Exception ex) {
                Log.CrashLog.TraceEvent(TraceEventType.Error, 0, "Compressing folder Exception: {0}", ex.ToString());
                throw;
            }
            return dumpFileName;
        }

        private static Regex s_CrashFileRegex;

        private static Regex CrashFileRegEx
        {
            get
            {
                if (s_CrashFileRegex == null) {
                    s_CrashFileRegex = new Regex(Crash.CrashPathRegEx);
                }
                return s_CrashFileRegex;
            }
        }

        private const long GbMultiplier = 1024 * 1024 * 1024;

        /// <summary>
        /// Removes old dump files from the default dump directory created when using <see cref="CreateDump()"/>.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">
        /// The platform is not supported, so delete operations can't be performed.
        /// </exception>
        /// <remarks>
        /// Over time, the number of files in the dump may take a significant amount of space. This method allows to
        /// programmatically remove old dumps and large dumps as disk space is reduced.
        /// <para>
        /// A log might be required to be deleted, but if there is a file system error, it will be skipped.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1854:Unused assignments should be removed",
            Justification = "Kept in case ordering is changed to reduce possible bugs")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value",
            Justification = "Kept in case ordering is changed to reduce possible bugs")]
        public static void CleanUpDump()
        {
            string dumpFolder;
            try {
                dumpFolder = Crash.GetCrashFolder();
            } catch (PlatformNotSupportedException) {
                return;
            }
            if (!Directory.Exists(dumpFolder)) return;

            Regex crashFileRegex = CrashFileRegEx;
            IList<FileSystemInfo> crashCandidates = new List<FileSystemInfo>();

            DirectoryInfo directory;
            try {
                directory = new DirectoryInfo(dumpFolder);
            } catch (PathTooLongException) {
                return;
            }
            DriveInfo drive = new DriveInfo(directory.Root.FullName);

            DirectoryInfo[] subDirs = directory.GetDirectories();
            if (subDirs != null) {
                foreach (DirectoryInfo subDir in subDirs) {
                    if (crashFileRegex.IsMatch(subDir.Name)) {
                        crashCandidates.Add(subDir);
                    }
                }
            }

            FileInfo[] files = directory.GetFiles();
            if (files != null) {
                foreach (FileInfo file in files) {
                    string nameNoExt = Path.GetFileNameWithoutExtension(file.Name);
                    if (crashFileRegex.IsMatch(nameNoExt)) {
                        crashCandidates.Add(file);
                    }
                }
            }

            // Delete everything more than 45 days old
            crashCandidates = CleanUpDumpOld(45, crashCandidates);

            // Keep the 40 newest sets of logs
            crashCandidates = CleanUpDumpKeepNewest(40, crashCandidates);

            // 1000MB or 10% should be minimum free space, but keep the last 5 files always
            crashCandidates = CleanUpKeepSpace(
                drive,
                5 * GbMultiplier, 1,     // Reserve is 5GB or 4% whichever is larger (e.g. 100GB = 5GB reserve; 1000GB = 10GB)
                1 * GbMultiplier, 5,     // Don't exceed 1GB if more than 5 files
                crashCandidates);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S3241:Methods should not return values that are never used",
            Justification = "General function, return value should remain in case ordering changes in caller")]
        private static IList<FileSystemInfo> CleanUpDumpOld(int ageDays, IList<FileSystemInfo> candidates)
        {
            if (candidates.Count == 0) return candidates;

            // Remove all logs/directories that are older than 45 days old
            IList<FileSystemInfo> remaining = new List<FileSystemInfo>();
            DateTime now = DateTime.UtcNow;
            foreach (var candidate in candidates) {
                if (now.Subtract(candidate.CreationTimeUtc).TotalDays > ageDays) {
                    Dump.Archive.FileSystem.Delete(candidate);
                } else {
                    remaining.Add(candidate);
                }
            }
            return remaining;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S3241:Methods should not return values that are never used",
            Justification = "General function, return value should remain in case ordering changes in caller")]
        private static IList<FileSystemInfo> CleanUpDumpKeepNewest(int count, IList<FileSystemInfo> candidates)
        {
            if (candidates.Count == 0) return candidates;

            IList<FileSystemInfo> remaining = new List<FileSystemInfo>();
            int candidateCount = candidates.Count;
            var created = from candidate in candidates orderby candidate.CreationTime select candidate;
            foreach (var candidate in created) {
                if (candidateCount > count) {
                    Dump.Archive.FileSystem.Delete(candidate);
                    --candidateCount;
                } else {
                    remaining.Add(candidate);
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S3241:Methods should not return values that are never used",
            Justification = "General function, return value should remain in case ordering changes in caller")]
        private static IList<FileSystemInfo> CleanUpKeepSpace(DriveInfo drive, long minDiskFree, int minPercent, long maxUsage, int minFiles, IList<FileSystemInfo> candidates)
        {
            if (candidates.Count == 0) return candidates;

            Dictionary<FileSystemInfo, long> sizes = new Dictionary<FileSystemInfo, long>();
            long totalSize = 0;
            foreach (var candidate in candidates) {
                long size = Dump.Archive.FileSystem.GetSize(candidate);
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
                    Dump.Archive.FileSystem.Delete(candidate);
                    totalSize -= sizes[candidate];
                    --candidateCount;
                } else {
                    remaining.Add(candidate);
                }
            }
            return remaining;
        }
    }
}
