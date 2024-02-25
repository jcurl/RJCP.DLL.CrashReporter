namespace RJCP.Diagnostics.Crash.Dumpers.NetVersion
{
    /// <summary>
    /// The .NET Framework type
    /// </summary>
    internal enum DotNetVersionType
    {
        /// <summary>
        /// .NET Framework (Legacy).
        /// </summary>
        NetFx,

        /// <summary>
        /// .NET Core.
        /// </summary>
        NetCore,

        /// <summary>
        /// Mono .NET Framework.
        /// </summary>
        Mono
    }
}
