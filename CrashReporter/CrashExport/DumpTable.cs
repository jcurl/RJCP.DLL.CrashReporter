namespace RJCP.Diagnostics.CrashExport
{
    using System;
    using System.Collections.Generic;
#if NET45
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// Support class for implementing <see cref="IDumpTable"/>.
    /// </summary>
    public abstract class DumpTable : IDumpTable
    {
        /// <summary>
        /// Writes the header for the table.
        /// </summary>
        /// <param name="header">The header.</param>
        public void DumpHeader(params string[] header)
        {
            DumpHeader((IEnumerable<string>)header);
        }

        /// <summary>
        /// Writes the header for the table.
        /// </summary>
        /// <param name="header">The header.</param>
        public abstract void DumpHeader(IEnumerable<string> header);

        /// <summary>
        /// Writes the row of properties/values to the current table.
        /// </summary>
        /// <param name="row">The row, a collection of key/value pairs that should be written.</param>
        public abstract void DumpRow(IEnumerable<KeyValuePair<string, string>> row);

        /// <summary>
        /// Flushes all unwritten values to the output stream.
        /// </summary>
        public abstract void Flush();

#if NET45
        /// <summary>
        /// Writes the header for the table.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <returns></returns>
        public Task DumpHeaderAsync(params string[] header)
        {
            return DumpHeaderAsync((IEnumerable<string>)header);
        }

        /// <summary>
        /// Writes the header for the table asynchronously.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <returns>
        /// An awaitable task.
        /// </returns>
        public abstract Task DumpHeaderAsync(IEnumerable<string> header);

        /// <summary>
        /// Writes the row of properties/values to the current table asynchronously.
        /// </summary>
        /// <param name="row">The row, a collection of key/value pairs that should be written.</param>
        /// <returns>
        /// An awaitable task.
        /// </returns>
        public abstract Task DumpRowAsync(IEnumerable<KeyValuePair<string, string>> row);

        /// <summary>
        /// Flushes all unwritten values to the output stream asynchronously.
        /// </summary>
        /// <returns>
        /// An awaitable task.
        /// </returns>
        public abstract Task FlushAsync();
#endif

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Nothing to dispose. User should override if they need to dispose something.
        }
    }
}
