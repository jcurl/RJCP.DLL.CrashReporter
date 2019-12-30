namespace RJCP.Diagnostics.CrashData.NetVersion
{
    /// <summary>
    /// Interface INetVersion for getting the version of .NET installed.
    /// </summary>
    internal interface INetVersion
    {
        /// <summary>
        /// Returns true if the version information contains valid (even if partially) information.
        /// </summary>
        /// <value><see langword="true"/> if this instance is valid; otherwise, <see langword="false"/>.</value>
        bool IsValid { get; }

        /// <summary>
        /// Gets the version string for the .NET version installed.
        /// </summary>
        /// <value>The .NET version string.</value>
        string Version { get; }

        /// <summary>
        /// Gets the description of the .NET version installed.
        /// </summary>
        /// <value>The .NET version description.</value>
        string Description { get; }
    }
}
