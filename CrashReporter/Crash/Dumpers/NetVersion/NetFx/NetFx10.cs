namespace RJCP.Diagnostics.Crash.Dumpers.NetVersion.NetFx
{
    using Microsoft.Win32;

    internal sealed class NetFx10 : INetVersion
    {
        internal NetFx10(string version, string key)
        {
            if (version.StartsWith("v")) {
#if NETFRAMEWORK
                Version = version.Substring(1);
#else
                Version = version[1..];
#endif
            } else {
                Version = version;
            }
            GetNetFxDetails(key);
        }

        private void GetNetFxDetails(string key)
        {
            string fullKeyPath = string.Format(@"SOFTWARE\Microsoft\NET Framework Setup\Product\{0}", key);
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(fullKeyPath)) {
                Package = registryKey.GetValue("Package", "").ToString();
                NetVersion = registryKey.GetValue("Version", "").ToString();

                Description = string.Format(".NET Framework {0} {1}", NetVersion, Package);
                IsValid = true;
            }
        }

        public string Package { get; private set; }

        public string NetVersion { get; private set; }

        public bool IsValid { get; private set; }

        public string Description { get; private set; }

        public string Version { get; private set; }
    }
}
