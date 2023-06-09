namespace RJCP.Diagnostics.Crash.Export.Xml
{
    using System;
    using System.Text;

    internal static class XmlExtensions
    {
        /// <summary>
        /// Converts any character which is not allowed according to XML 1.0 to a textual representation.
        /// </summary>
        /// <param name="input">The input that should be sanitized.</param>
        /// <returns>A sanitized string.</returns>
        /// <remarks>
        /// This method is used to sanitize output which is intended for human readable input. It is not intended for
        /// storing information that might need to be reverted back - for that you should use a different encoding,
        /// such as Base64 encoding for general binary input data.
        /// <para>See XML Recommendation 1.0 Section 2.2 Characters.</para>
        /// <para>Char ::= #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]</para>
        /// </remarks>
        public static string SanitizeXml10(string input)
        {
            if (input == null) return string.Empty;
            StringBuilder sb = null;
            int pos = 0;
            int cp = 0;
            foreach (char c in input) {
                if (!IsValidXml10(c)) {
                    if (sb == null) sb = new StringBuilder(input.Length + 128);
#if NETFRAMEWORK
                    if (pos > cp) sb.Append(input.Substring(cp, pos - cp));
#else
                    if (pos > cp) sb.Append(input.AsSpan(cp, pos - cp));
#endif
                    cp = pos + 1;
                    sb.Append(EncodeChar(c));
                }
                pos++;
            }
            if (sb == null) return input;
#if NETFRAMEWORK
            if (pos > cp) sb.Append(input.Substring(cp, pos - cp));
#else
            if (pos > cp) sb.Append(input.AsSpan(cp, pos - cp));
#endif
            return sb.ToString();
        }

        private static bool IsValidXml10(char c)
        {
            if (c <= 8) return false;
            if (c == 11 || c == 12) return false;
            if (c >= 14 && c < 32) return false;
            if (c >= 0xD800 && c < 0xE000) return false;
            if (c == 0xFFFE || c == 0xFFFF) return false;
            return true;
        }

        private static string EncodeChar(char c)
        {
            return string.Format("[0x{0:X2}]", (int)c);
        }
    }
}
