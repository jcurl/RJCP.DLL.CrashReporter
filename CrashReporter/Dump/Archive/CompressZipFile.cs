namespace RJCP.Diagnostics.Dump.Archive
{
    using System;
    using System.IO;
    using Ionic.Zip;

    internal static class Compress
    {
        public static bool CompressFolder(string path)
        {
            if (path == null) return false;
            if (!Directory.Exists(path)) return false;

            DirectoryInfo sourceDir = new DirectoryInfo(path);
            string zipFileName = string.Format("{0}.zip", path);
            using (ZipFile zipFile = new ZipFile()) {
                ZipDirectoryTree(zipFile, sourceDir, Path.GetFileName(path));
                zipFile.Save(zipFileName);
            }
            return true;
        }

        private static void ZipDirectoryTree(ZipFile zipFile, DirectoryInfo sourceDir, string zipDir)
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
                    zipFile.AddFile(file.FullName, zipDir);
                }
            }

            DirectoryInfo[] subDirs = sourceDir.GetDirectories();
            foreach (DirectoryInfo subDir in subDirs) {
                string subDirPath = Path.Combine(zipDir, subDir.Name);
                ZipDirectoryTree(zipFile, subDir, subDirPath);
            }
        }
    }
}
