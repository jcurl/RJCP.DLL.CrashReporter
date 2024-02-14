namespace RJCP.Diagnostics.Crash.Dumpers.NetVersion.Runtime
{
    using System;
#if NET6_0_OR_GREATER
    using System.Runtime.InteropServices;
#else
    using System.Reflection;
#endif

    internal sealed class NetFx : INetVersion
    {
        public NetFx()
        {
            Version = Environment.Version.ToString();
#if NET6_0_OR_GREATER
            Description = RuntimeInformation.FrameworkDescription;
#else
            // This information is available in .NET 4.7.1 and later. But as we also target earlier frameworks, get this
            // information dynamically.
            Type rti = Type.GetType("System.Runtime.InteropServices.RuntimeInformation");
            if (rti is not null) {
                PropertyInfo property = rti.GetProperty("FrameworkDescription");
                Description = property.GetValue(null, null) as string;
            }

            Description ??= ".NET Environment Runtime";
#endif
            IsValid = true;
        }

        public string Description { get; private set; }

        public bool IsValid { get; private set; }

        public string Version { get; private set; }
    }
}
