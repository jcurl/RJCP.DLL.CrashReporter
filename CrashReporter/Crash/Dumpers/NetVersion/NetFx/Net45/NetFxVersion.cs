namespace RJCP.Diagnostics.Crash.Dumpers.NetVersion.NetFx.Net45
{
    using System;

    internal sealed class NetFxVersion
    {
        internal NetFxVersion(Version version, string description)
        {
            Version = version;
            Description = description;
        }

        public Version Version { get; private set; }

        public string Description { get; private set; }
    }
}
