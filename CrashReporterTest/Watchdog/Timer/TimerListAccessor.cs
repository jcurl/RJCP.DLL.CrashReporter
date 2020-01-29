namespace RJCP.Diagnostics.Watchdog.Timer
{
    using System.Collections.Generic;
    using RJCP.CodeQuality;

    public class TimerListAccessor : AccessorBase
    {
        private const string AssemblyName = "RJCP.Diagnostics.CrashReporter";
        private const string TypeName = "RJCP.Diagnostics.Watchdog.Timer.TimerList";
        public static readonly PrivateType AccType = new PrivateType(AssemblyName, TypeName);

        public TimerListAccessor(ITimerSource timerSource) : base(AccType, timerSource) { }

        public int Count
        {
            get { return (int)GetFieldOrProperty(nameof(Count)); }
        }

        public int Active
        {
            get { return (int)GetFieldOrProperty(nameof(Active)); }
        }

        public void Add(string item, int timeout)
        {
            Invoke(nameof(Add), item, timeout);
        }

        public void Change(string item, int timeout)
        {
            Invoke(nameof(Change), item, timeout);
        }

        public bool Remove(string item)
        {
            return (bool)Invoke(nameof(Remove), item);
        }

        public bool Contains(string item)
        {
            return (bool)Invoke(nameof(Contains), item);
        }

        public int NextExpiryOffset()
        {
            return (int)Invoke(nameof(NextExpiryOffset));
        }

        public IEnumerable<string> ExpungeExpired()
        {
            return (IEnumerable<string>)Invoke(nameof(ExpungeExpired));
        }

        public void Clear()
        {
            Invoke(nameof(Clear));
        }
    }
}
