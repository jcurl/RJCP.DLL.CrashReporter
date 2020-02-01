namespace RJCP.Diagnostics.Config.CrashReporter.EnvVar
{
    internal class TokenString : IToken
    {
        public TokenString(string value)
        {
            Value = value;
        }

        public string Value { get; private set; }

        public override string ToString()
        {
            return Value;
        }
    }
}
