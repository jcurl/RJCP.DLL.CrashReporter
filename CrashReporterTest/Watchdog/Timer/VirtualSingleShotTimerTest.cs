namespace RJCP.Diagnostics.Watchdog.Timer
{
    using System.Threading;
    using NUnit.Framework;

    [TestFixture]
    public class VirtualSingleShotTimerTest
    {
        [TestCase]
        public void UpdateNotSet()
        {
            VirtualTimerSource vTimerSource = new(-5);
            VirtualSingleShotTimer vTimer = new(vTimerSource);

            bool alarm = false;
            vTimer.AlarmEvent += (s, e) => { alarm = true; };

            vTimerSource.UpdateClock(4);        // clock is now -1.
            Assert.That(alarm, Is.False);
            vTimerSource.UpdateClock(1);        // Clock is now 0. It might see 0 as an alarm. It shouldn't.
            Assert.That(alarm, Is.False);
            vTimerSource.UpdateClock(1);        // Clock is now 1. It might see cross over as an alarm. It shouldn't.
            Assert.That(alarm, Is.False);
        }

        [TestCase]
        public void CheckAlarm()
        {
            VirtualTimerSource vTimerSource = new(-5);
            VirtualSingleShotTimer vTimer = new(vTimerSource);

            bool alarm = false;
            vTimer.AlarmEvent += (s, e) => { alarm = true; };

            vTimer.SetDelay(1);
            vTimerSource.UpdateClock(1);
            Assert.That(alarm, Is.True);
        }

        [TestCase]
        public void CheckAlarmWrapOver()
        {
            VirtualTimerSource vTimerSource = new(-5);
            VirtualSingleShotTimer vTimer = new(vTimerSource);

            bool alarm = false;
            vTimer.AlarmEvent += (s, e) => { alarm = true; };

            vTimer.SetDelay(10);
            vTimerSource.UpdateClock(1);
            Assert.That(alarm, Is.False);
            vTimerSource.UpdateClock(8);
            Assert.That(alarm, Is.False);
            vTimerSource.UpdateClock(2);
            Assert.That(alarm, Is.True);
        }

        [TestCase]
        public void DisableAlarm()
        {
            VirtualTimerSource vTimerSource = new(-5);
            VirtualSingleShotTimer vTimer = new(vTimerSource);

            bool alarm = false;
            vTimer.AlarmEvent += (s, e) => { alarm = true; };

            vTimer.SetDelay(10);
            vTimerSource.UpdateClock(1);
            Assert.That(alarm, Is.False);
            vTimerSource.UpdateClock(8);
            Assert.That(alarm, Is.False);

            vTimer.SetDelay(Timeout.Infinite);
            vTimerSource.UpdateClock(0);
            Assert.That(alarm, Is.False);
        }
    }
}
