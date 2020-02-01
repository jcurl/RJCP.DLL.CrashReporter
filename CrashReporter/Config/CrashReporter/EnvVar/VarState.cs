namespace RJCP.Diagnostics.Config.CrashReporter.EnvVar
{
    internal enum VarState
    {
        Expansion,
        Token,
        VariableStart,
        Variable
    }
}
