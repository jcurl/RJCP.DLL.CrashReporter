namespace RJCP.Diagnostics.Crash.Export
{
    using System;
    using System.Collections.Generic;
#if NET45_OR_GREATER || NET6_0_OR_GREATER
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
        /// <param name="row">The row which also contains the list of headers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="row"/> is <see langword="null"/>.</exception>
        public void DumpHeader(DumpRow row)
        {
            ThrowHelper.ThrowIfNull(row);
            DumpHeader(row.GetHeader());
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
        /// <exception cref="ArgumentNullException"><paramref name="row"/> is <see langword="null"/>.</exception>
        public void DumpRow(DumpRow row)
        {
            ThrowHelper.ThrowIfNull(row);
            DumpRow(row.GetRow());
            row.Clear();
        }

        /// <summary>
        /// Writes the row of properties/values to the current table.
        /// </summary>
        /// <param name="row">The row, a collection of key/value pairs that should be written.</param>
        public abstract void DumpRow(IDictionary<string, string> row);

        /// <summary>
        /// Flushes all unwritten values to the output stream.
        /// </summary>
        public abstract void Flush();

#if NET45_OR_GREATER || NET6_0_OR_GREATER
        /// <summary>
        /// Writes the header for the table.
        /// </summary>
        /// <param name="row">The row which also contains the list of headers.</param>
        /// <returns>An awaitable task.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="row"/> is <see langword="null"/>.</exception>
        public Task DumpHeaderAsync(DumpRow row)
        {
            ThrowHelper.ThrowIfNull(row);
            return DumpHeaderAsync(row.GetHeader());
        }

        /// <summary>
        /// Writes the header for the table asynchronously.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <returns>An awaitable task.</returns>
        public abstract Task DumpHeaderAsync(IEnumerable<string> header);

        /// <summary>
        /// Writes the row of properties/values to the current table asynchronously.
        /// </summary>
        /// <param name="row">The row, a collection of key/value pairs that should be written.</param>
        /// <returns>An awaitable task.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="row"/> is <see langword="null"/>.</exception>
        public Task DumpRowAsync(DumpRow row)
        {
            ThrowHelper.ThrowIfNull(row);
            return DumpRowInternalAsync(row);
        }

        private async Task DumpRowInternalAsync(DumpRow row)
        {
            await DumpRowAsync(row.GetRow());
            row.Clear();
        }

        /// <summary>
        /// Writes the row of properties/values to the current table asynchronously.
        /// </summary>
        /// <param name="row">The row, a collection of key/value pairs that should be written.</param>
        /// <returns>An awaitable task.</returns>
        public abstract Task DumpRowAsync(IDictionary<string, string> row);

        /// <summary>
        /// Flushes all unwritten values to the output stream asynchronously.
        /// </summary>
        /// <returns>An awaitable task.</returns>
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
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release
        /// only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            // Nothing to dispose. User should override if they need to dispose something.
        }
    }
}
