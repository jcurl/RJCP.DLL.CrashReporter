namespace RJCP.Diagnostics.Dump
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using Native.Win32;
    using RJCP.Core.Environment;
#if !NETFRAMEWORK
    using System.Reflection;
#endif

    /// <summary>
    /// Methods to allow core dumps for application debugging.
    /// </summary>
    public static class Core
    {
        private static FileStream OpenFile(string path)
        {
            if (File.Exists(path)) {
                return File.Open(path, FileMode.Append);
            } else {
                return File.Create(path);
            }
        }

        /// <summary>
        /// Creates a crash dump using the DebugHelp library.
        /// </summary>
        /// <param name="path">The path to write the dump file.</param>
        /// <returns><see langword="true"/> if the mini-dump was successful.</returns>
        /// <remarks>
        /// This method is only supported on Windows. On other operating systems, the call is ignored and no crash dump
        /// is created. No exception is raised to keep the same behavior as if the program worked (i.e. a crash dump
        /// would be made for debugging purposes and then continue, it stands to reason on foreign operating systems we
        /// can't debug, so just let the program continue).
        /// </remarks>
        public static bool MiniDump(string path)
        {
            return MiniDump(path, CoreType.MiniDump);
        }

        /// <summary>
        /// Creates a crash dump using the DebugHelp library.
        /// </summary>
        /// <param name="path">The path to write the dump file.</param>
        /// <param name="dumpType">Select the type of crash dump to perform.</param>
        /// <returns><see langword="true"/> if the mini-dump was successful.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> is a zero-length string, contains only white space, or contains one or more
        /// invalid characters as defined by InvalidPathChars.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length. For example, on
        /// Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260
        /// characters.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission.
        /// <para>- or -</para>
        /// <paramref name="path"/> specified a file that is read-only
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// The specified <paramref name="path"/> is invalid (for example, it is on an unmapped drive).
        /// </exception>
        /// <exception cref="IOException">
        /// An I/O error occurred while creating the file given by <paramref name="path"/>.
        /// </exception>
        /// <exception cref="NotSupportedException"><paramref name="path"/> is in an invalid format.</exception>
        /// <remarks>
        /// This method is only supported on Windows. On other operating systems, the call is ignored and no crash
        /// dump is created. No exception is raised to keep the same behavior as if the program worked (i.e. a crash
        /// dump would be made for debugging purposes and then continue, it stands to reason on foreign operating
        /// systems we can't debug, so just let the program continue).
        /// </remarks>
        public static bool MiniDump(string path, CoreType dumpType)
        {
            if (!Platform.IsWinNT()) return false;
            if (path == null) return false;
            if (dumpType == CoreType.None) return false;

            // For some useful information about making dumps:
            //  http://www.debuginfo.com/articles/effminidumps.html
            //
            // There's a ClrDump too, but it's not visible by default
            //  http://www.debuginfo.com/tools/clrdump.html

            DbgHelp.MINIDUMP_TYPE dbgDumpType;
            switch (dumpType) {
            case CoreType.MiniDump:
                dbgDumpType = DbgHelp.MINIDUMP_TYPE.MiniDumpNormal;
                break;
            case CoreType.FullHeap:
#if NET40
                // Windows XP supports .NET 4.0, but not later.
                if (Environment.OSVersion.Version.Major <= 5) {
                    dbgDumpType =
                        DbgHelp.MINIDUMP_TYPE.MiniDumpWithFullMemory |
                        DbgHelp.MINIDUMP_TYPE.MiniDumpWithHandleData |
                        DbgHelp.MINIDUMP_TYPE.MiniDumpWithUnloadedModules;
                } else {
                    // Windows Vista or later
                    dbgDumpType =
                        DbgHelp.MINIDUMP_TYPE.MiniDumpWithFullMemory |
                        DbgHelp.MINIDUMP_TYPE.MiniDumpWithFullMemoryInfo |
                        DbgHelp.MINIDUMP_TYPE.MiniDumpWithHandleData |
                        DbgHelp.MINIDUMP_TYPE.MiniDumpWithThreadInfo |
                        DbgHelp.MINIDUMP_TYPE.MiniDumpWithUnloadedModules;
                }
#else
                // Windows Vista or later
                dbgDumpType =
                    DbgHelp.MINIDUMP_TYPE.MiniDumpWithFullMemory |
                    DbgHelp.MINIDUMP_TYPE.MiniDumpWithFullMemoryInfo |
                    DbgHelp.MINIDUMP_TYPE.MiniDumpWithHandleData |
                    DbgHelp.MINIDUMP_TYPE.MiniDumpWithThreadInfo |
                    DbgHelp.MINIDUMP_TYPE.MiniDumpWithUnloadedModules;
#endif
                break;
            default:
                dbgDumpType = DbgHelp.MINIDUMP_TYPE.MiniDumpNormal;
                break;
            }

            using (FileStream fsToDump = OpenFile(path)) {
#if NETFRAMEWORK
                if (Environment.OSVersion.Version.Major <= 5) {
                    // Windows XP - no exception information, as this otherwise results in zero sized core dumps
                    return DbgHelp.MiniDumpWriteDump(
                        Kernel32.GetCurrentProcess(),
                        Kernel32.GetCurrentProcessId(),
                        fsToDump.SafeFileHandle,
                        dbgDumpType,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        IntPtr.Zero);
                } else {
                    DbgHelp.MINIDUMP_EXCEPTION_INFORMATION miniDumpInfo =
                    new DbgHelp.MINIDUMP_EXCEPTION_INFORMATION {
                        ClientPointers = 0,
                        ExceptionPointers = Marshal.GetExceptionPointers(),
                        ThreadId = Kernel32.GetCurrentThreadId()
                    };
                    return DbgHelp.MiniDumpWriteDump(
                        Kernel32.GetCurrentProcess(),
                        Kernel32.GetCurrentProcessId(),
                        fsToDump.SafeFileHandle,
                        dbgDumpType,
                        ref miniDumpInfo,
                        //IntPtr.Zero,
                        IntPtr.Zero,
                        IntPtr.Zero);
                }
#else
                DbgHelp.MINIDUMP_EXCEPTION_INFORMATION miniDumpInfo =
                    new DbgHelp.MINIDUMP_EXCEPTION_INFORMATION {
                        ClientPointers = 0,
                        ExceptionPointers = GetExceptionPointers(),
                        ThreadId = Kernel32.GetCurrentThreadId()
                    };

                return DbgHelp.MiniDumpWriteDump(
                    Kernel32.GetCurrentProcess(),
                    Kernel32.GetCurrentProcessId(),
                    fsToDump.SafeFileHandle,
                    dbgDumpType,
                    ref miniDumpInfo,
                    IntPtr.Zero,
                    IntPtr.Zero);
#endif
            }
        }

#if !NETFRAMEWORK
        private static IntPtr GetExceptionPointers()
        {
            Type marshal = typeof(Marshal);
            MethodInfo pointers = marshal.GetMethod("GetExceptionPointers");
            if (pointers != null)
                return (IntPtr)pointers.Invoke(null, null);

            return IntPtr.Zero;
        }
#endif
    }
}
