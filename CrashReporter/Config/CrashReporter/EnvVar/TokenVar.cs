namespace RJCP.Diagnostics.Config.CrashReporter.EnvVar
{
    using System;
    using System.IO;
    using RJCP.Core.Environment;

    internal class TokenVar : IToken
    {
        public TokenVar(string envVar)
        {
            if (envVar.Equals("LOCALAPPDATA", StringComparison.Ordinal)) {
                Value = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            } else if (envVar.Equals("APPDATA", StringComparison.Ordinal)) {
                Value = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            } else if (envVar.Equals("HOME", StringComparison.Ordinal)) {
                if (Platform.IsWinNT()) {
                    string userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
                    if (!string.IsNullOrWhiteSpace(userProfile)) {
                        Value = userProfile;
                    } else {
                        Value = string.Format("{0}{1}",
                            Environment.GetEnvironmentVariable("HOMEDRIVE"),
                            Environment.GetEnvironmentVariable("HOMEPATH"));
                    }
                } else {
                    Value = Environment.GetEnvironmentVariable("HOME");
                }
            } else if (envVar.Equals("CWD", StringComparison.Ordinal)) {
                Value = Environment.CurrentDirectory;
            } else if (envVar.Equals("APPDIR", StringComparison.Ordinal)) {
                string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                Value = Path.GetDirectoryName(assemblyPath);
            } else {
                Value = Environment.GetEnvironmentVariable(envVar);
            }
        }

        public string Value { get; private set; }

        public override string ToString()
        {
            return Value;
        }
    }
}
