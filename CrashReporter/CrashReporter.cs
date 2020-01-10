namespace RJCP.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.ExceptionServices;
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
                Source.TraceEvent(TraceEventType.Information, 0, "First Chance Exception: {0}", args.Exception.Message);
                StackTrace stack = new StackTrace(true);
                Source.TraceEvent(TraceEventType.Information, 0, stack.ToString());
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
                Exception ex = args.ExceptionObject as Exception;
                if (ex == null) {
                    Source.TraceEvent(TraceEventType.Critical, 0, "Unhandled Exception: {0}", args.ExceptionObject.ToString());
                } else {
                    Source.TraceEvent(TraceEventType.Critical, 0, "Unhandled Exception: {0}", ex.Message);
                }
                StackTrace stack = new StackTrace(true);
                Source.TraceEvent(TraceEventType.Critical, 0, stack.ToString());
            }
            CreateDump();
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
        /// Creates a dump with with the minidump specified.
        /// </summary>
        /// <param name="coreType">Type of the core dump to create.</param>
        /// <returns>The name of the file that was generated, so the user might be notified.</returns>
        /// <remarks>
        /// You can use this method to generate your own core dump as required, for example, if an exception is caught
        /// (so that the <see cref="AppDomain.UnhandledException"/> won't be raised).
        /// </remarks>
        public static string CreateDump(CoreType coreType)
        {
            string path = Crash.Data.Dump();
            if (path == null) return null;

            string dumpDir;
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

            string dumpFileName = null;
            try {
                dumpFileName = Dump.Archive.Compress.CompressFolder(dumpDir);
                if (dumpFileName != null && File.Exists(dumpFileName)) {
                    // Only delete if compression was successful.
                    Dump.Archive.FileSystem.DeleteFolder(dumpDir);
                }
            } catch (IOException) {
                // Problem compressing or deleting. Ignore
            } catch (PlatformNotSupportedException) {
                // Can't delete the folder...
            }
            return dumpFileName;
        }
    }
}
