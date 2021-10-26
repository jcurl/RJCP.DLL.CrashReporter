namespace RJCP.Diagnostics.CrashData.NetVersion.Runtime
{
    using System;
#if NETSTANDARD
    using System.Runtime.InteropServices;
#else
    using System.Reflection;
#endif

    internal class NetFx : INetVersion
    {
        public NetFx()
        {
            Version = Environment.Version.ToString();
#if NETSTANDARD
            Description = RuntimeInformation.FrameworkDescription;
#else
            // This information is available in .NET 4.7.1 and later. But as we also target earlier frameworks, get this
            // information dynamically.
            Type rti = Type.GetType("System.Runtime.InteropServices.RuntimeInformation");
            if (rti != null) {
                PropertyInfo property = rti.GetProperty("FrameworkDescription");
                Description = property.GetValue(null, null) as string;
            }

            if (Description == null)
                Description = ".NET Environment Runtime";
#endif
            IsValid = true;
        }

        public string Description { get; private set; }

        public bool IsValid { get; private set; }

        public string Version { get; private set; }
    }
}
