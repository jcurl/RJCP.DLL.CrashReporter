namespace RJCP.Diagnostics.Config.CrashReporter
{
    using System.Collections.Generic;
    using System.Text;

    internal static class Parser
    {
        /// <summary>
        /// Parses a string which may contain environment variables.
        /// </summary>
        /// <param name="envVar">The string to parse.</param>
        /// <returns>
        /// An string with expanded environment variables. A return value of <see langword="null"/> indicates a
        /// parsing error.
        /// </returns>
        /// <remarks>
        /// The ABNF (RFC5234) form for the string is:
        /// <list type="bullet">
        /// <item>expansion = *( var / chars / dirSep )</item>
        /// <item>var = ${ [string] }</item>
        /// <item>string = firstChar *chars</item>
        /// <item>firstChar = a-z / A-z / _</item>
        /// <item>chars = firstChar / - / 0-9</item>
        /// <item>dirSep = "\" / "/"</item>
        /// </list>
        /// </remarks>
        public static string ParseEnvVar(string envVar)
        {
            if (string.IsNullOrEmpty(envVar)) return string.Empty;

            // As this is a small ABNF syntax, we translate it directly into a state machine in code. The routine is
            // not on any critical path, so there are no real optimizations applied for the sake of readability.
            List<EnvVar.IToken> tokens = new();
            StringBuilder component = new();
            EnvVar.VarState state = EnvVar.VarState.Expansion;
            int l = envVar.Length;
            for (int i = 0; i < l; i++) {
                char c = envVar[i];
                switch (state) {
                case EnvVar.VarState.Expansion:
                    if (IsChar(c)) {
                        component.Append(c);
                    } else if (IsDirSep(c)) {
                        component.Append(System.IO.Path.DirectorySeparatorChar);
                    } else if (IsTokenStart(c)) {
                        if (component.Length > 0) {
                            tokens.Add(new EnvVar.TokenString(component.ToString()));
                            component.Clear();
                        }
                        state = EnvVar.VarState.Token;
                    } else {
                        return null;
                    }
                    break;
                case EnvVar.VarState.Token:
                    if (IsVarStart(c)) {
                        state = EnvVar.VarState.VariableStart;
                    } else {
                        return null;
                    }
                    break;
                case EnvVar.VarState.VariableStart:
                    if (IsFirstChar(c)) {
                        component.Append(c);
                        state = EnvVar.VarState.Variable;
                    } else if (IsVarEnd(c)) {
                        state = EnvVar.VarState.Expansion;
                    } else {
                        return null;
                    }
                    break;
                case EnvVar.VarState.Variable:
                    if (IsChar(c)) {
                        component.Append(c);
                    } else if (IsVarEnd(c)) {
                        tokens.Add(new EnvVar.TokenVar(component.ToString()));
                        component.Clear();
                        state = EnvVar.VarState.Expansion;
                    } else {
                        return null;
                    }
                    break;
                }
            }

            if (state != EnvVar.VarState.Expansion) return null;
            if (component.Length > 0) {
                tokens.Add(new EnvVar.TokenString(component.ToString()));
                component.Clear();
            }

            foreach (EnvVar.IToken token in tokens) {
                component.Append(token.ToString());
            }
            return component.ToString();
        }

        private static bool IsFirstChar(char c)
        {
            if (char.IsLetter(c)) return true;
            if (c == '_') return true;
            return false;
        }

        private static bool IsChar(char c)
        {
            if (IsFirstChar(c)) return true;
            if (c is >= '0' and <= '9') return true;
            if (c == '-') return true;
            return false;
        }

        private static bool IsTokenStart(char c)
        {
            return (c == '$');
        }

        private static bool IsVarStart(char c)
        {
            return (c == '{');
        }

        private static bool IsVarEnd(char c)
        {
            return (c == '}');
        }

        private static bool IsDirSep(char c)
        {
            return c is '/' or '\\';
        }
    }
}
