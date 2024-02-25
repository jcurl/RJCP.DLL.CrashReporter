namespace RJCP.Diagnostics.Crash.Dumpers.NetVersion
{
    using System;

    /// <summary>
    /// Interface INetVersionMono for getting the version of .NET Mono installed.
    /// </summary>
    internal interface INetVersionMono : INetVersion
    {
        /// <summary>
        /// Gets the path to the Mono binary.
        /// </summary>
        string MonoPath { get; }

        /// <summary>
        /// Gets the version of the MSCorLib.DLL file installed.
        /// </summary>
        Version MsCorLibVersion { get; }

        /// <summary>
        /// Gets the architecture for the Mono runtime.
        /// </summary>
        string Architecture { get; }
    }
}
