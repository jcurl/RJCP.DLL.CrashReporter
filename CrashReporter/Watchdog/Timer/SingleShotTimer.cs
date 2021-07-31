namespace RJCP.Diagnostics.Watchdog.Timer
{
    using System;
    using System.Threading;

    /// <summary>
    /// A single shot timer for notification of the next watchdog timeout.
    /// </summary>
    public class SingleShotTimer : ISingleShotTimer
    {
        private readonly Timer m_Timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleShotTimer"/> class.
        /// </summary>
        /// <remarks>
        /// The timer is not yet activated. See <see cref="SetDelay(int)"/>.
        /// </remarks>
        public SingleShotTimer()
        {
            m_Timer = new Timer(TimerCallback);
        }

        /// <summary>
        /// Sets the timeout for the next <see cref="AlarmEvent" />.
        /// </summary>
        /// <param name="timeout">The delay in milliseconds for the next timeout event.</param>
        public void SetDelay(int timeout)
        {
            if (timeout < 0) {
                m_Timer.Change(Timeout.Infinite, Timeout.Infinite);
                return;
            }

            if (timeout == 0) {
                m_Timer.Change(Timeout.Infinite, Timeout.Infinite);
                OnAlarmEvent(this, new EventArgs());
                return;
            }

            m_Timer.Change(timeout, Timeout.Infinite);
        }

        private void TimerCallback(object data)
        {
            OnAlarmEvent(this, new EventArgs());
        }

        /// <summary>
        /// Occurs when a timeout occurs from a previous call to <see cref="SetDelay(int)" />.
        /// </summary>
        public event EventHandler AlarmEvent;

        /// <summary>
        /// Handles the <see cref="AlarmEvent" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnAlarmEvent(object sender, EventArgs args)
        {
            EventHandler handler = AlarmEvent;
            if (handler != null) handler(sender, args);
        }
    }
}
