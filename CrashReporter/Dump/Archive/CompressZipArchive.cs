namespace RJCP.Diagnostics.Dump.Archive
{
    using System;
    using System.IO;
    using System.IO.Compression;

    internal static class Compress
    {
        public static string CompressFolder(string path)
        {
            if (path == null) return null;
            if (!Directory.Exists(path)) return null;

            DirectoryInfo sourceDir = new DirectoryInfo(path);
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
            if (files != null) {
                foreach (FileInfo file in files) {
                    string zipEntry = Path.Combine(zipDir, file.Name);
                    archive.CreateEntryFromFile(file.FullName, zipEntry, CompressionLevel.Optimal);
                }
            }

            DirectoryInfo[] subDirs = sourceDir.GetDirectories();
            foreach (DirectoryInfo subDir in subDirs) {
                string subDirPath = Path.Combine(zipDir, subDir.Name);
                ZipDirectoryTree(archive, subDir, subDirPath);
            }
        }
    }
}
