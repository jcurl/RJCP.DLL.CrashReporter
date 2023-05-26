namespace RJCP.Diagnostics.Dump.Archive
{
    using System;
    using System.IO;
    using System.Threading;
    using RJCP.Core.Environment;

#if NET45_OR_GREATER || NETSTANDARD
    using System.Runtime.ExceptionServices;
#endif

    internal static class FileSystem
    {
        private const int DeleteMaxTime = 5000;
        private const int DeletePollInterval = 100;

        public static void Delete(FileSystemInfo fsInfo)
        {
            Delete(fsInfo, false);
        }

        /// <summary>
        /// Deletes the specified file or folder.
        /// </summary>
        /// <param name="fsInfo">
        /// The file system information object, which is a <see cref="DriveInfo"/> or a <see cref="FileInfo"/>.
        /// </param>
        /// <param name="throwOnError">If <see langword="true"/>, throw an exception and abort immediately.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="fsInfo"/> Must be a DirectoryInfo or a FileInfo to delete.
        /// </exception>
        public static void Delete(FileSystemInfo fsInfo, bool throwOnError)
        {
            if (fsInfo is DirectoryInfo dir) {
                DeleteFolder(dir, throwOnError);
            } else if (fsInfo is FileInfo file) {
                IOAction(() => { DeleteFile(file); }, throwOnError);
            } else {
                throw new ArgumentException("Must be a DirectoryInfo or a FileInfo to delete", nameof(fsInfo));
            }
        }

        /// <summary>
        /// Deletes the folder.
        /// </summary>
        /// <param name="path">The path to the folder to delete.</param>
        /// <remarks>
        /// Tries to delete the folder, and continues on some file system errors.
        /// </remarks>
        public static void DeleteFolder(string path)
        {
            DeleteFolder(new DirectoryInfo(path), false);
        }

        /// <summary>
        /// Deletes the folder.
        /// </summary>
        /// <param name="path">The path to the folder to delete.</param>
        /// <param name="throwOnError">If <see langword="true"/>, throw an exception and abort immediately.</param>
        /// <exception cref="PlatformNotSupportedException">Platform is not supported.</exception>
        /// <exception cref="DirectoryNotFoundException">
        /// The path is invalid, such as being on an unmapped drive.
        /// <para>- or -</para>
        /// Instead of deleting a directory, the path given was for a file.
        /// </exception>
        /// <exception cref="IOException">
        /// The specified file is in use;
        /// <para>- or -</para>
        /// There is an open handle on the file, and the operating system is Windows XP or earlier. This open handle
        /// can result from enumerating directories and files;
        /// <para>- or -</para>
        /// Tried to delete a directory but it was a file (file system race condition that another process made a
        /// file);
        /// <para>- or -</para>
        /// The directory is not empty (file system race condition that another process created a file);
        /// <para>- or -</para>
        /// The directory is being used by another process.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, file name, or both (including subdirectories) exceed the system-defined maximum
        /// length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names
        /// must be less than 260 characters;
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission;
        /// <para>- or -</para>
        /// There is a read-only file.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        public static void DeleteFolder(string path, bool throwOnError)
        {
            DeleteFolder(new DirectoryInfo(path), throwOnError);
        }

        /// <summary>
        /// Deletes the folder.
        /// </summary>
        /// <param name="dir">The directory folder to delete.</param>
        /// <remarks>
        /// Tries to delete the folder, and continues on some file system errors.
        /// </remarks>
        public static void DeleteFolder(DirectoryInfo dir)
        {
            DeleteFolder(dir, false);
        }

        /// <summary>
        /// Deletes the folder.
        /// </summary>
        /// <param name="dir">The directory folder to delete.</param>
        /// <param name="throwOnError">If <see langword="true"/>, throw an exception and abort immediately.</param>
        /// <exception cref="PlatformNotSupportedException">Platform is not supported.</exception>
        /// <exception cref="DirectoryNotFoundException">
        /// The path is invalid, such as being on an unmapped drive.
        /// <para>- or -</para>
        /// A path deleted was a file, when it was expected to be a directory (file system race condition that another
        /// process created a file)
        /// </exception>
        /// <exception cref="IOException">
        /// The specified file is in use;
        /// <para>- or -</para>
        /// There is an open handle on the file, and the operating system is Windows XP or earlier. This open handle
        /// can result from enumerating directories and files;
        /// <para>- or -</para>
        /// Tried to delete a directory but it was a file (file system race condition that another process made a
        /// file);
        /// <para>- or -</para>
        /// The directory is not empty (file system race condition that another process created a file);
        /// <para>- or -</para>
        /// The directory is being used by another process.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, file name, or both (including subdirectories) exceed the system-defined maximum
        /// length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names
        /// must be less than 260 characters;
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission;
        /// <para>- or -</para>
        /// There is a read-only file.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        public static void DeleteFolder(DirectoryInfo dir, bool throwOnError)
        {
            if (!dir.Exists) return;

            FileInfo[] files = dir.GetFiles();
            if (files != null) {
                foreach (FileInfo file in files) {
                    IOAction(() => { DeleteFile(file); }, throwOnError);
                }
            }

            DirectoryInfo[] subDirs = dir.GetDirectories();
            foreach (DirectoryInfo subDir in subDirs) {
                IOAction(() => { DeleteFolder(subDir, throwOnError); }, throwOnError);
            }

            IOAction(() => { DeleteEmptyFolder(dir); }, throwOnError);
        }

        private static void IOAction(Action action, bool throwOnError)
        {
            try {
                action();
            } catch (DirectoryNotFoundException) {
                if (throwOnError) throw;
                // Ignore, try to continue to delete other files.
            } catch (PathTooLongException) {
                if (throwOnError) throw;
                // Ignore, try to continue to delete other files.
            } catch (IOException) {
                if (throwOnError) throw;
                // Ignore, try to continue to delete other files.
            } catch (UnauthorizedAccessException) {
                if (throwOnError) throw;
                // Ignore, try to continue to delete other files.
            } catch (System.Security.SecurityException) {
                if (throwOnError) throw;
                // Ignore, try to continue to delete other files.
            }
        }

        private static void DeleteEmptyFolder(DirectoryInfo dir)
        {
            if (Platform.IsWinNT()) {
                DeleteEmptyFolderWindows(dir);
            } else if (Platform.IsUnix()) {
                DeleteEmptyFolderUnix(dir);
            } else {
                throw new PlatformNotSupportedException();
            }
        }

        private static void DeleteEmptyFolderUnix(DirectoryInfo dir)
        {
            Directory.Delete(dir.FullName);
            if (!Directory.Exists(dir.FullName)) return;
            string message = string.Format("Directory '{0}' couldn't be deleted", dir.Name);
            throw new IOException(message);
        }

        private static void DeleteEmptyFolderWindows(DirectoryInfo dir)
        {
            int elapsed;
            int tickCount = Environment.TickCount;
            int deletePollIntervalExp = 5;

#if NET45_OR_GREATER || NETSTANDARD
            ExceptionDispatchInfo lastException;
#else
            Exception lastException;
#endif
            do {
            lastException = null;
                try {
                    Directory.Delete(dir.FullName);
                } catch (UnauthorizedAccessException ex) {
                    // Occurs on Windows if a file in the directory is open.
#if NET45_OR_GREATER || NETSTANDARD
                    lastException = ExceptionDispatchInfo.Capture(ex);
#else
                    lastException = ex;
#endif
                } catch (IOException ex) {
                    // Occurs on Windows if a file in the directory is open (or on Windows XP someone is enumerating the
                    // directory).
#if NET45_OR_GREATER || NETSTANDARD
                    lastException = ExceptionDispatchInfo.Capture(ex);
#else
                    lastException = ex;
#endif
                }
                if (!Directory.Exists(dir.FullName)) return;
                Thread.Sleep(deletePollIntervalExp);
                if (deletePollIntervalExp < DeletePollInterval) {
                    deletePollIntervalExp = Math.Min(deletePollIntervalExp * 2, DeletePollInterval);
                }
                elapsed = unchecked(Environment.TickCount - tickCount);
            } while (elapsed < DeleteMaxTime);

            if (lastException != null) {
#if NET45_OR_GREATER || NETSTANDARD
                lastException.Throw();
#else
                throw lastException;
#endif
            }
            string message = string.Format("Directory '{0}' couldn't be deleted", dir.Name);
            throw new IOException(message);
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="path">The file to delete.</param>
        /// <exception cref="PlatformNotSupportedException">Platform is not supported.</exception>
        /// <exception cref="DirectoryNotFoundException">
        /// The specified path is invalid (for example, it is on an unmapped drive).
        /// </exception>
        /// <exception cref="IOException">
        /// The specified file is in use.
        /// <para>- or -</para>
        /// There is an open handle on the file, and the operating system is Windows XP or earlier. This open handle
        /// can result from enumerating directories and files.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length. For example, on
        /// Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260
        /// characters.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission.
        /// <para>- or -</para>
        /// <paramref name="path"/> is a read-only file.
        /// </exception>
        public static void DeleteFile(string path)
        {
            DeleteFile(new FileInfo(path));
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="file">The file to delete.</param>
        /// <exception cref="PlatformNotSupportedException">Platform is not supported.</exception>
        /// <exception cref="DirectoryNotFoundException">
        /// The specified path is invalid (for example, it is on an unmapped drive).
        /// </exception>
        /// <exception cref="IOException">
        /// The specified file is in use.
        /// <para>- or -</para>
        /// There is an open handle on the file, and the operating system is Windows XP or earlier. This open handle
        /// can result from enumerating directories and files.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length. For example, on
        /// Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260
        /// characters.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission.
        /// <para>- or -</para>
        /// <paramref name="file"/> is a read-only file.
        /// </exception>
        public static void DeleteFile(FileInfo file)
        {
            if (!file.Exists) return;

            if (Platform.IsWinNT()) {
                DeleteFileWindows(file);
            } else if (Platform.IsUnix()) {
                DeleteFileUnix(file);
            } else {
                throw new PlatformNotSupportedException();
            }
        }

        private static void DeleteFileUnix(FileInfo file)
        {
            File.Delete(file.FullName);
            if (!File.Exists(file.FullName)) return;
            string message = string.Format("File '{0}' couldn't be deleted", file.Name);
            throw new IOException(message);
        }

        private static void DeleteFileWindows(FileInfo file)
        {
            int elapsed;
            int tickCount = Environment.TickCount;
            int deletePollIntervalExp = 5;

#if NET45_OR_GREATER || NETSTANDARD
            ExceptionDispatchInfo lastException;
#else
            Exception lastException;
#endif
            do {
                lastException = null;
                try {
                    File.Delete(file.FullName);
                } catch (UnauthorizedAccessException ex) {
                    // Occurs on Windows if the file is opened by a process.
#if NET45_OR_GREATER || NETSTANDARD
                    lastException = ExceptionDispatchInfo.Capture(ex);
#else
                    lastException = ex;
#endif
                } catch (IOException ex) {
                    // Occurs on Windows if the file is opened by a process.
#if NET45_OR_GREATER || NETSTANDARD
                    lastException = ExceptionDispatchInfo.Capture(ex);
#else
                    lastException = ex;
#endif
                }
                if (!File.Exists(file.FullName)) return;
                Thread.Sleep(deletePollIntervalExp);
                if (deletePollIntervalExp < DeletePollInterval) {
                    deletePollIntervalExp = Math.Min(deletePollIntervalExp * 2, DeletePollInterval);
                }
                elapsed = unchecked(Environment.TickCount - tickCount);
            } while (elapsed < DeleteMaxTime);

            if (lastException != null) {
#if NET45_OR_GREATER || NETSTANDARD
                lastException.Throw();
#else
                throw lastException;
#endif
            }
            string message = string.Format("File '{0}' couldn't be deleted", file.Name);
            throw new IOException(message);
        }

        public static long GetSize(FileSystemInfo fsInfo)
        {
            if (fsInfo is DirectoryInfo dir) {
                return GetFolderSize(dir);
            } else if (fsInfo is FileInfo file) {
                return file.Length;
            }
            return 0;
        }

        public static long GetFolderSize(DirectoryInfo dir)
        {
            if (!dir.Exists) {
                string message = string.Format("Directory not found '{0}'", dir.FullName);
                throw new DirectoryNotFoundException(message);
            }

            long size = 0;
            FileInfo[] files = null;
            try {
                files = dir.GetFiles();
            } catch (UnauthorizedAccessException) {
                // Ignore, as it shouldn't happen
            } catch (DirectoryNotFoundException) {
                // Ignore, as the directory was deleted while enumerating
            } catch (PathTooLongException) {
                // Ignore, as it shouldn't happen
            }
            if (files != null) {
                foreach (FileInfo file in files) {
                    size += file.Length;
                }
            }

            DirectoryInfo[] subDirs;
            try {
                subDirs = dir.GetDirectories();
                foreach (DirectoryInfo subDir in subDirs) {
                    size += GetFolderSize(subDir);
                }
            } catch (DirectoryNotFoundException) {
                // Ignore, as the directory was deleted while enumerating
            } catch (PathTooLongException) {
                // Ignore, as it shouldn't happen
            }

            return size;
        }
    }
}
