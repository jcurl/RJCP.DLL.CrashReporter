namespace RJCP.Diagnostics.Config.CrashReporter
{
    using System;

    /// <summary>
    /// Configuration on managing the dump directory.
    /// </summary>
    public class DumpDir
    {
        internal DumpDir()
        {
            Path = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CrashDumps");
            AgeDays = 45;
            MaxLogs = 40;
            ReserveFree = 5;
            ReserveFreePercent = 1;
            MaxDirSize = 1;
            MaxDirSizeMinLogs = 5;
        }

        internal DumpDir(DumpDirElement dumpDir) : this()
        {
            string path = Parser.ParseEnvVar(dumpDir.Path);
            if (path != null) Path = path;

            if (int.TryParse(dumpDir.AgeDays, out int ageDays)) {
                if (ageDays <= 0) ageDays = 1;
                AgeDays = ageDays;
            }

            if (int.TryParse(dumpDir.MaxLogs, out int maxLogs)) {
                if (maxLogs <= 0) maxLogs = 1;
                MaxLogs = maxLogs;
            }

            if (int.TryParse(dumpDir.ReserveFree, out int reserveFree)) {
                if (reserveFree <= 0) reserveFree = 1;
                ReserveFree = reserveFree;
            }

            if (int.TryParse(dumpDir.ReserveFreePercent, out int reserveFreePercent)) {
                if (reserveFreePercent < 0) reserveFreePercent = 0;
                if (reserveFreePercent > 99) reserveFreePercent = 99;
                ReserveFreePercent = reserveFreePercent;
            }

            if (int.TryParse(dumpDir.MaxDirSize, out int maxDirSize)) {
                if (maxDirSize <= 0) maxDirSize = 1;
                MaxDirSize = maxDirSize;
            }

            if (int.TryParse(dumpDir.MaxDirSizeMinLogs, out int maxDirSizeMinLogs)) {
                if (maxDirSizeMinLogs <= 0) maxDirSizeMinLogs = 1;
                MaxDirSizeMinLogs = maxDirSizeMinLogs;
            }
        }

        /// <summary>
        /// Gets the directory path where to store the dump files.
        /// </summary>
        /// <value>The directory path where to store the dump files.</value>
        public string Path { get; private set; }

        /// <summary>
        /// Gets the age, in days, from when to start removing old dump files.
        /// </summary>
        /// <value>The age days when to start removing old dump files.</value>
        public int AgeDays { get; private set; }

        /// <summary>
        /// Gets the maximum logs to keep in the dump directory.
        /// </summary>
        /// <value>The maximum logs of logs to keep in the dump directory.</value>
        public int MaxLogs { get; private set; }

        /// <summary>
        /// Gets the reserved free space in gigabytes.
        /// </summary>
        /// <value>The reserved free space in gigabytes.</value>
        public int ReserveFree { get; private set; }

        /// <summary>
        /// Gets the reserved free space as a percentage.
        /// </summary>
        /// <value>The reserved free space as a percentage.</value>
        public int ReserveFreePercent { get; private set; }

        /// <summary>
        /// Gets the maximum size of the dump directory.
        /// </summary>
        /// <value>The maximum size of the dump directory.</value>
        public int MaxDirSize { get; private set; }

        /// <summary>
        /// Gets the minimum number of logs to keep if the max size of the directory is exceeded.
        /// </summary>
        /// <value>The minimum number of logs to keep if the max size of the directory is exceeded.</value>
        public int MaxDirSizeMinLogs { get; private set; }
    }
}
