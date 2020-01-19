namespace RJCP.Diagnostics.Watchdog.Timer
{
    /// <summary>
    /// An object describing when an item is to expire.
    /// </summary>
    internal sealed class TimerItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimerItem"/> class.
        /// </summary>
        /// <param name="item">The name of the item.</param>
        public TimerItem(string item)
        {
            Item = item;
        }

        /// <summary>
        /// Gets the name of the item.
        /// </summary>
        /// <value>The name of the item.</value>
        public string Item { get; private set; }

        /// <summary>
        /// The absolute time when this item is to expire.
        /// </summary>
        /// <value>The expiry time in milliseconds.</value>
        public int Expiry { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TimerItem"/> is enabled.
        /// </summary>
        /// <value>Is <see langword="true"/> if enabled; otherwise, <see langword="false"/>.</value>
        public bool Enabled { get; set; } = true;
    }
}
