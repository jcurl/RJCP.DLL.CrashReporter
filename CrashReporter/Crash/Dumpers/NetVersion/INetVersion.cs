namespace RJCP.Diagnostics.Crash.Dumpers.NetVersion
{
    using System;

    /// <summary>
    /// Interface INetVersion for getting the version of .NET installed.
    /// </summary>
    internal interface INetVersion
    {
        /// <summary>
        /// Returns <see langword="true"/> if the version information contains valid (even if partially) information.
        /// </summary>
        /// <value><see langword="true"/> if this instance is valid; otherwise, <see langword="false"/>.</value>
        bool IsValid { get; }

        /// <summary>
        /// The .NET Version Type.
        /// </summary>
        DotNetVersionType VersionType { get; }

        /// <summary>
        /// Gets the version of the Framework that can be used for comparison.
        /// </summary>
        /// <value>The .NET version that can be used for comparison.</value>
        /// <remarks>
        /// The version returned contains only the <see cref="Version.Major"/> and <see cref="Version.Minor"/> fields.
        /// </remarks>
        Version FrameworkVersion { get; }

        /// <summary>
        /// Gets the version of the installation.
        /// </summary>
        /// <value>The .NET installation version.</value>
        Version InstallVersion { get; }

        /// <summary>
        /// Gets the description of the .NET version installed.
        /// </summary>
        /// <value>The .NET version description.</value>
        string Description { get; }
    }
}
