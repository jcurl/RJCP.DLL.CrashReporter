namespace RJCP.Diagnostics.Crash.Dumpers.NetVersion.Runtime
{
    using System;
    using System.Reflection;

    internal sealed class Mono : INetVersion
    {
        public Mono()
        {
            GetMonoVersion();
        }

        private void GetMonoVersion()
        {
            Type monoType = Type.GetType("Mono.Runtime");
            if (monoType == null) {
                IsValid = false;
                return;
            }

            Version = Environment.Version.ToString();

            MethodInfo displayName = monoType.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
            if (displayName != null) {
                Description = string.Format("Mono {0}", displayName.Invoke(null, null));
            }

            IsValid = true;
        }

        public bool IsValid { get; private set; }

        public string Version { get; private set; }

        public string Description { get; private set; }
    }
}
