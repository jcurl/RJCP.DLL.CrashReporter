namespace RJCP.Diagnostics.Dump.Archive
{
    using System;
    using System.IO;
    using System.Threading;

    internal static class FileSystem
    {
        private const int DeleteMaxTime = 5000;
        private const int DeletePollInterval = 100;

        public static void DeleteFolder(string path)
        {
            if (!Directory.Exists(path)) return;

            DeleteSubDirectory(new DirectoryInfo(path));
        }

        private static void DeleteSubDirectory(DirectoryInfo dir)
        {
            FileInfo[] files = null;
            try {
                files = dir.GetFiles();
            } catch (UnauthorizedAccessException) {
                // Ignore, as it shouldn't happen
            } catch (DirectoryNotFoundException) {
                // Ignore, as it shouldn't happen
            } catch (PathTooLongException) {
                // Ignore, as it shouldn't happen
            }
            if (files != null) {
                foreach (FileInfo file in files) {
                    if (Platform.IsWinNT()) {
                        DeleteFileWindows(file);
                    } else if (Platform.IsUnix()) {
                        DeleteFileUnix(file);
                    } else {
                        throw new PlatformNotSupportedException();
                    }
                }
            }

            DirectoryInfo[] subDirs = dir.GetDirectories();
            foreach (DirectoryInfo subDir in subDirs) {
                DeleteSubDirectory(subDir);
            }

            DeleteEmptyDirectory(dir);
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
            Exception lastException;
            do {
                lastException = null;
                try {
                    File.Delete(file.FullName);
                } catch (UnauthorizedAccessException ex) {
                    // Occurs on Windows if the file is opened by a process.
                    lastException = ex;
                } catch (IOException ex) {
                    // Occurs on Windows if the file is opened by a process.
                    lastException = ex;
                }
                if (!File.Exists(file.FullName)) return;
                Thread.Sleep(deletePollIntervalExp);
                if (deletePollIntervalExp < DeletePollInterval) {
                    deletePollIntervalExp = Math.Min(deletePollIntervalExp * 2, DeletePollInterval);
                }
                elapsed = unchecked(Environment.TickCount - tickCount);
            } while (elapsed < DeleteMaxTime);

            if (lastException != null) throw lastException;
            string message = string.Format("File '{0}' couldn't be deleted", file.Name);
            throw new IOException(message);
        }

        private static void DeleteEmptyDirectory(DirectoryInfo dir)
        {
            if (Platform.IsWinNT()) {
                DeleteEmptyDirectoryWindows(dir);
            } else if (Platform.IsUnix()) {
                DeleteEmptyDirectoryUnix(dir);
            } else {
                throw new PlatformNotSupportedException();
            }
        }

        private static void DeleteEmptyDirectoryUnix(DirectoryInfo dir)
        {
            Directory.Delete(dir.FullName);
            if (!Directory.Exists(dir.FullName)) return;
            string message = string.Format("Directory '{0}' couldn't be deleted", dir.Name);
            throw new IOException(message);
        }

        private static void DeleteEmptyDirectoryWindows(DirectoryInfo dir)
        {
            int elapsed;
            int tickCount = Environment.TickCount;
            int deletePollIntervalExp = 5;
            Exception lastException;
            do {
                lastException = null;
                try {
                    Directory.Delete(dir.FullName);
                } catch (UnauthorizedAccessException ex) {
                    // Occurs on Windows if a file in the directory is open.
                    lastException = ex;
                } catch (IOException ex) {
                    // Occurs on Windows if a file in the directory is open (or on Windows XP someone is enumerating the
                    // directory).
                    lastException = ex;
                }
                if (!Directory.Exists(dir.FullName)) return;
                Thread.Sleep(deletePollIntervalExp);
                if (deletePollIntervalExp < DeletePollInterval) {
                    deletePollIntervalExp = Math.Min(deletePollIntervalExp * 2, DeletePollInterval);
                }
                elapsed = unchecked(Environment.TickCount - tickCount);
            } while (elapsed < DeleteMaxTime);

            if (lastException != null) throw lastException;
            string message = string.Format("Directory '{0}' couldn't be deleted", dir.Name);
            throw new IOException(message);
        }
    }
}
