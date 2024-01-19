// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0074:Use compound assignment", Justification = ".NET Core only feature")]
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "P/Invoke", Scope = "namespaceanddescendants", Target = "~N:RJCP.Diagnostics.Native")]
[assembly: SuppressMessage("Performance", "CA1866:Use StartsWith overload that takes a char", Justification = ".NET Core only feature")]
