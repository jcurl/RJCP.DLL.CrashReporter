namespace RJCP.Diagnostics.Crash
{
    /// <summary>
    /// Specify a dump type.
    /// </summary>
    public enum CoreType
    {
        /// <summary>
        /// Don't create a core dump.
        /// </summary>
        None,

        /// <summary>
        /// On Windows, perform a dump that contains information about the system, process, modules (libraries), thread
        /// states, stack and optionally exception details.
        /// </summary>
        MiniDump,

        /// <summary>
        /// On Windows, perform a full memory dump of the process containing all information. This produces the largest
        /// memory dump, but allows debugging of objects on the heap, shared memory contents and global variables.
        /// </summary>
        FullHeap
    }
}
