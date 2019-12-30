namespace RJCP.Diagnostics.CrashData.NetVersion.Runtime
{
    using System;

    internal class NetFx : INetVersion
    {
        public NetFx()
        {
            Version = Environment.Version.ToString();
            Description = ".NET Environment Runtime";
            IsValid = true;
        }

        public string Description { get; private set; }

        public bool IsValid { get; private set; }

        public string Version { get; private set; }
    }
}
