namespace RJCP.Diagnostics.CrashData.NetVersion.NetFx
{
    using System;
    using System.Security;
    using System.Text;
    using Microsoft.Win32;

    // See https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed
    internal class NetFxLegacy : INetVersion
    {
        internal NetFxLegacy(string key)
        {
            try {
                GetNetFxDetails(key);
            } catch (SecurityException) {
                IsValid = false;
            } catch (Exception) {
                IsValid = false;
            }
        }

        private void GetNetFxDetails(string key)
        {
            if (!key.StartsWith("v")) {
                IsValid = false;
                return;
            }

            string fullKeyPath = string.Format(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\{0}", key);
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(fullKeyPath)) {
                IsValid = false;
                if (registryKey == null) return;

                string installed = registryKey.GetValue("Install", "").ToString();
                if (installed == null || installed != "1") return;

                string[] path = key.Split('\\');

                string netVersion = (string)registryKey.GetValue("Version");
                if (netVersion == null) netVersion = path[0].Substring(1);
                NetVersion = new Version(netVersion);

                string servicePack = registryKey.GetValue("SP", "").ToString();
                if (servicePack.Equals("0")) servicePack = string.Empty;

                StringBuilder version = new StringBuilder();
                version.Append(path[0].Substring(1));
                if (path.Length >= 2) version.Append(" ").Append(path[1]);
                if (!string.IsNullOrEmpty(servicePack)) version.Append(" SP").Append(servicePack);
                Version = version.ToString();

                StringBuilder description = new StringBuilder();
                description.Append(".NET Framework ").Append(netVersion);
                if (path.Length >= 2) description.Append(" Profile ").Append(path[1]);
                if (!string.IsNullOrEmpty(servicePack)) description.Append(" SP").Append(servicePack);
                Description = description.ToString();

                IsValid = true;
            }
        }

        public Version NetVersion { get; private set; }

        public bool IsValid { get; private set; }

        public string Description { get; private set; }

        public string Version { get; private set; }
    }
}
