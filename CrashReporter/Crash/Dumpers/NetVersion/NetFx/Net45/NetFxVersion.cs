namespace RJCP.Diagnostics.Crash.Dumpers.NetVersion.NetFx.Net45
{
    internal class NetFxVersion
    {
        internal NetFxVersion(string version, string description)
        {
            Version = version;
            Description = description;
        }

        public string Version { get; private set; }

        public string Description { get; private set; }
    }
}
