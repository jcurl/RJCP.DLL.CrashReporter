namespace RJCP.Diagnostics.Crash.Archive
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Reflection;

    internal static class Compress
    {
        public static string CompressFolder(string path)
        {
            if (path is null) return null;
            if (!Directory.Exists(path)) return null;

            DirectoryInfo sourceDir = new(path);
            string zipFileName = string.Format("{0}.zip", path);
            using (ZipArchive archive = ZipFile.Open(zipFileName, ZipArchiveMode.Create)) {
                ZipDirectoryTree(archive, sourceDir, Path.GetFileName(path));
            }
            return zipFileName;
        }

        private static void ZipDirectoryTree(ZipArchive archive, DirectoryInfo sourceDir, string zipDir)
        {
            FileInfo[] files = null;
            try {
                files = sourceDir.GetFiles();
            } catch (UnauthorizedAccessException) {
                // Ignore, as it shouldn't happen
            } catch (DirectoryNotFoundException) {
                // Ignore, as it shouldn't happen
            } catch (PathTooLongException) {
                // Ignore, as it shouldn't happen
            }
            if (files is not null) {
                foreach (FileInfo file in files) {
                    string zipEntry = Path.Combine(zipDir, file.Name);
                    _ = CreateEntryFromFile(archive, file.FullName, zipEntry);
                }
            }

            DirectoryInfo[] subDirs = sourceDir.GetDirectories();
            foreach (DirectoryInfo subDir in subDirs) {
                string subDirPath = Path.Combine(zipDir, subDir.Name);
                ZipDirectoryTree(archive, subDir, subDirPath);
            }
        }

        private static ZipArchiveEntry CreateEntryFromFile(ZipArchive archive, string sourceFileName, string entryName)
        {
            ZipArchiveEntry entry = archive.CreateEntryFromFile(sourceFileName, entryName, CompressionLevel.Optimal);

            // This only exists on .NET 4.7.2 or later
            // Permissions 0640 = 1 1010 0000
            // 0100640 = 1 000 000 110 100 000 = 81A0
            //
            // Bitfields, see https://www.gnu.org/software/libc/manual/html_node/Permission-Bits.html
            //   S_IRUSR = 0400  S_IWUSR = 0200  S_IXUSR = 0100
            //   S_IRGRP = 0040  S_IWGRP = 0020  S_IXGRP = 0010
            //   S_IROTH = 0004  S_IWOTH = 0002  S_IXOTH = 0001
            //   S_ISUID = 4000  S_ISGID = 2000  S_ISVTX = 1000 (sticky)
            //
            //   S_IFIFO  0010000  /* named pipe (fifo) */
            //   S_IFCHR  0020000  /* character special */
            //   S_IFDIR  0040000  /* directory */
            //   S_IFBLK  0060000  /* block special */
            //   S_IFREG  0100000  /* regular */
            //   S_IFLNK  0120000  /* symbolic link */
            //   S_IFSOCK 0140000  /* socket */
            const int unixAttributes = 0x81A0;

#if NET6_0_OR_GREATER || NET471_OR_GREATER
            entry.ExternalAttributes = (entry.ExternalAttributes & 0xFFFF) | (unixAttributes << 16);
#else
            Type rti = typeof(ZipArchiveEntry);

            // This information is available in .NET 4.7.1 and later. But as we also target earlier frameworks, get this
            // information dynamically.
            PropertyInfo property = rti.GetProperty("ExternalAttributes");
            if (property is null) return entry;

            int attributes = (int)property.GetValue(entry);
            attributes = (attributes & 0xFFFF) | (unixAttributes << 16);
            property.SetValue(entry, attributes);
#endif
            return entry;
        }
    }
}
