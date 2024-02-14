namespace RJCP.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.IO;

    internal static class ProcessInfo
    {
        private static readonly object ProcessInfoLock = new();
        private static string s_ProcessName;

#if NETFRAMEWORK
        private static int s_ProcessId;
#endif

        public static string ProcessName
        {
            get
            {
                if (s_ProcessName is null) {
                    lock (ProcessInfoLock) {
                        if (s_ProcessName is null) {
#if NET6_0_OR_GREATER
                            string name = Environment.ProcessPath;
                            if (File.Exists(name)) {
                                s_ProcessName = Path.GetFileName(name);
                            } else {
                                s_ProcessName = "UNKNOWN";
                            }
#else
                            using (Process proc = Process.GetCurrentProcess()) {
                                try {
                                    string name = proc.ProcessName;
                                    s_ProcessName = Path.GetFileName(name);
                                } catch (InvalidOperationException) {
                                    s_ProcessName = "UNKNOWN";
                                } catch (NotSupportedException) {
                                    s_ProcessName = "UNKNOWN_REMOTE";
                                }
                            }
#endif
                        }
                    }
                }
                return s_ProcessName;
            }
        }

        public static int ProcessId
        {
            get
            {
#if NET6_0_OR_GREATER
                return Environment.ProcessId;
#else
                if (s_ProcessId == 0) {
                    lock (ProcessInfoLock) {
                        if (s_ProcessId == 0) {
                            using (Process proc = Process.GetCurrentProcess()) {
                                try {
                                    s_ProcessId = proc.Id;
                                } catch (InvalidOperationException) {
                                    s_ProcessId = -1;
                                }
                            }
                        }
                    }
                }
                return s_ProcessId;
#endif
            }
        }
    }
}
