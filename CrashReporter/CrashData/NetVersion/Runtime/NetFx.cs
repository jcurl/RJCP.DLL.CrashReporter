namespace RJCP.Diagnostics.CrashData.NetVersion.Runtime
{
    using System;
#if NETSTANDARD
    using System.Runtime.InteropServices;
#endif

    internal class NetFx : INetVersion
    {
        public NetFx()
        {
            Version = Environment.Version.ToString();
#if !NETSTANDARD
            Description = ".NET Environment Runtime";
#else
            Description = RuntimeInformation.FrameworkDescription;
#endif
            IsValid = true;
        }

        public string Description { get; private set; }

        public bool IsValid { get; private set; }

        public string Version { get; private set; }
    }
}
