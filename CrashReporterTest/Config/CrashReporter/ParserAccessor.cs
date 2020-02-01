namespace RJCP.Diagnostics.Config.CrashReporter
{
    using CodeQuality;

    public static class ParserAccessor
    {
        private static readonly PrivateType AccessorType =
            new PrivateType("RJCP.Diagnostics.CrashReporter", "RJCP.Diagnostics.Config.CrashReporter.Parser");

        public static string ParseEnvVar(string envVar)
        {
            return (string)AccessorBase.InvokeStatic(AccessorType, nameof(ParseEnvVar), envVar);
        }
    }
}
