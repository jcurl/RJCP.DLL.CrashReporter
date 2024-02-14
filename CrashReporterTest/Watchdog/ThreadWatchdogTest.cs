namespace RJCP.Diagnostics.Watchdog
{
    using NUnit.Framework;

    [TestFixture]
    public class ThreadWatchdogTest
    {
        // By using different initial clocks, we aim to look for potential overflow bugs and initial condition errors.
        private static readonly int[] InitialClocks = new[] {
            -60000, -50000, -40000, -30000, -20000, -10000, 0, 10000, 20000, 30000, 40000, 50000, 60000
        };

        [TestCase]
        public void RegisterWatchdog()
        {
            Timer.VirtualTimerSource vTimerSource = new(0);
            Timer.VirtualSingleShotTimer vTimer = new(vTimerSource);
            ThreadWatchdog watchdog = new(vTimerSource, vTimer);
            Assert.That(watchdog.Register("1", 30000, 45000), Is.True);
        }

        [TestCaseSource(nameof(InitialClocks))]
        public void WatchdogWarningCritical(int initialClock)
        {
            Timer.VirtualTimerSource vTimerSource = new(initialClock);
            Timer.VirtualSingleShotTimer vTimer = new(vTimerSource);
            ThreadWatchdog watchdog = new(vTimerSource, vTimer);

            bool warning = false; bool critical = false;
            watchdog.WarningEvent += (s, e) => { warning = true; };
            watchdog.CriticalEvent += (s, e) => { critical = true; };

            Assert.That(watchdog.Register("1", 30000, 45000), Is.True);
            Assert.That(warning, Is.False);
            Assert.That(critical, Is.False);

            vTimerSource.UpdateClock(25000);  // 25sec of 30sec for warning.
            Assert.That(warning, Is.False);
            Assert.That(critical, Is.False);

            vTimerSource.UpdateClock(4999);   // 29.999s of 30s for warning.
            Assert.That(warning, Is.False);
            Assert.That(critical, Is.False);

            vTimerSource.UpdateClock(1);      // 30s of 30s for warning. Should trigger.
            Assert.That(warning, Is.True);
            Assert.That(critical, Is.False);
            warning = false;

            vTimerSource.UpdateClock(10000);  // 40s of 45s for critical.
            Assert.That(warning, Is.False);
            Assert.That(critical, Is.False);

            vTimerSource.UpdateClock(4999);   // 44.999s of 45s for critical.
            Assert.That(warning, Is.False);
            Assert.That(critical, Is.False);

            vTimerSource.UpdateClock(1);      // 45s of 45s for critical. Should trigger.
            Assert.That(warning, Is.False);
            Assert.That(critical, Is.True);
            critical = false;

            // No more events for this watchdog
            vTimerSource.UpdateClock(30000);  // 25sec of 30sec for warning.
            Assert.That(warning, Is.False);
            Assert.That(critical, Is.False);
        }

        [TestCaseSource(nameof(InitialClocks))]
        public void WatchdogNoWarningPing(int initialClock)
        {
            Timer.VirtualTimerSource vTimerSource = new(initialClock);
            Timer.VirtualSingleShotTimer vTimer = new(vTimerSource);
            ThreadWatchdog watchdog = new(vTimerSource, vTimer);

            bool warning = false; bool critical = false;
            watchdog.WarningEvent += (s, e) => { warning = true; };
            watchdog.CriticalEvent += (s, e) => { critical = true; };

            Assert.That(watchdog.Register("1", 30000, 45000), Is.True);
            Assert.That(warning, Is.False);
            Assert.That(critical, Is.False);

            vTimerSource.UpdateClock(25000);  // 25sec of 30sec for warning.
            Assert.That(warning, Is.False);
            Assert.That(critical, Is.False);

            watchdog.Ping("1");               // Should reset the timer back to 30s remaining
            vTimerSource.UpdateClock(10000);  // 10sec of 30sec for warning.
            Assert.That(warning, Is.False);
            Assert.That(critical, Is.False);

            vTimerSource.UpdateClock(20000);  // 35sec of 30sec for warning. Warning.
            Assert.That(warning, Is.True);
            Assert.That(critical, Is.False);

            vTimerSource.UpdateClock(20000);  // 55sec of 45sec for critical. Critical.
            Assert.That(warning, Is.True);
            Assert.That(critical, Is.True);
        }

        [TestCaseSource(nameof(InitialClocks))]
        public void WatchdogWarningPing(int initialClock)
        {
            Timer.VirtualTimerSource vTimerSource = new(initialClock);
            Timer.VirtualSingleShotTimer vTimer = new(vTimerSource);
            ThreadWatchdog watchdog = new(vTimerSource, vTimer);

            bool warning = false; bool critical = false;
            watchdog.WarningEvent += (s, e) => { warning = true; };
            watchdog.CriticalEvent += (s, e) => { critical = true; };

            Assert.That(watchdog.Register("1", 30000, 45000), Is.True);
            Assert.That(warning, Is.False);
            Assert.That(critical, Is.False);

            vTimerSource.UpdateClock(35000);  // 35sec of 30sec for warning. Warning.
            Assert.That(warning, Is.True);
            Assert.That(critical, Is.False);
            warning = false;

            watchdog.Ping("1");               // Should reset the timer back to 30s remaining and reenable the warning
            vTimerSource.UpdateClock(10000);  // 10sec of 30sec for warning.
            Assert.That(warning, Is.False);
            Assert.That(critical, Is.False);

            vTimerSource.UpdateClock(15000);  // 25sec of 30sec for warning.
            Assert.That(warning, Is.False);
            Assert.That(critical, Is.False);

            vTimerSource.UpdateClock(15000);  // 40sec of 30sec for warning. Warning.
            Assert.That(warning, Is.True);
            Assert.That(critical, Is.False);

            vTimerSource.UpdateClock(5000);   // 45sec of 45sec for critical. Critical.
            Assert.That(warning, Is.True);
            Assert.That(critical, Is.True);
        }

        [TestCaseSource(nameof(InitialClocks))]
        public void WatchdogWarningCriticalPing(int initialClock)
        {
            Timer.VirtualTimerSource vTimerSource = new(initialClock);
            Timer.VirtualSingleShotTimer vTimer = new(vTimerSource);
            ThreadWatchdog watchdog = new(vTimerSource, vTimer);

            bool warning = false; bool critical = false;
            watchdog.WarningEvent += (s, e) => { warning = true; };
            watchdog.CriticalEvent += (s, e) => { critical = true; };

            Assert.That(watchdog.Register("1", 30000, 45000), Is.True);
            Assert.That(warning, Is.False);
            Assert.That(critical, Is.False);

            vTimerSource.UpdateClock(35000);  // 35sec of 30sec for warning. Warning.
            Assert.That(warning, Is.True);
            Assert.That(critical, Is.False);
            warning = false;

            vTimerSource.UpdateClock(10000);  // 45sec of 45sec for critical. Critical.
            Assert.That(warning, Is.False);
            Assert.That(critical, Is.True);
            critical = false;

            vTimerSource.UpdateClock(5000);  // 50sec of 45sec for critical.
            Assert.That(warning, Is.False);
            Assert.That(critical, Is.False);

            watchdog.Ping("1");               // Should reset the timer back to 30s remaining and reenable the warning
            vTimerSource.UpdateClock(35000);  // 35sec of 30sec for warning. Warning.
            Assert.That(warning, Is.True);
            Assert.That(critical, Is.False);

            vTimerSource.UpdateClock(10000);  // 45sec of 45sec for critical. Critical.
            Assert.That(warning, Is.True);
            Assert.That(critical, Is.True);
        }

        [TestCaseSource(nameof(InitialClocks))]
        public void MultipleWatchdogsCritical(int initialClock)
        {
            Timer.VirtualTimerSource vTimerSource = new(initialClock);
            Timer.VirtualSingleShotTimer vTimer = new(vTimerSource);
            ThreadWatchdog watchdog = new(vTimerSource, vTimer);

            int warning = 0; int critical = 0;
            watchdog.WarningEvent += (s, e) => { warning++; };
            watchdog.CriticalEvent += (s, e) => { critical++; };

            Assert.That(watchdog.Register("1", 30000, 45000), Is.True);
            Assert.That(warning, Is.EqualTo(0));
            Assert.That(critical, Is.EqualTo(0));

            vTimerSource.UpdateClock(1000);        // 1s elapsed.
            Assert.That(watchdog.Register("2", 30000, 45000), Is.True);
            Assert.That(warning, Is.EqualTo(0));
            Assert.That(critical, Is.EqualTo(0));

            // Simulate that the process hangs for some really long time. 2 WD warnings should occur (one event though)
            vTimerSource.UpdateClock(31000);       // 32s elapsed.
            Assert.That(warning, Is.EqualTo(1));
            Assert.That(critical, Is.EqualTo(0));

            vTimerSource.UpdateClock(13000);       // 45s elapsed. "1" is now Critical.
            Assert.That(warning, Is.EqualTo(1));
            Assert.That(critical, Is.EqualTo(1));

            vTimerSource.UpdateClock(1000);        // 46s elapsed. "2" is now Critical.
            Assert.That(warning, Is.EqualTo(1));
            Assert.That(critical, Is.EqualTo(2));
        }

        [TestCaseSource(nameof(InitialClocks))]
        public void UnregisterBeforeWarning(int initialClock)
        {
            Timer.VirtualTimerSource vTimerSource = new(initialClock);
            Timer.VirtualSingleShotTimer vTimer = new(vTimerSource);
            ThreadWatchdog watchdog = new(vTimerSource, vTimer);

            int warning = 0; int critical = 0;
            watchdog.WarningEvent += (s, e) => { warning++; };
            watchdog.CriticalEvent += (s, e) => { critical++; };

            Assert.That(watchdog.Register("1", 30000, 45000), Is.True);
            Assert.That(warning, Is.EqualTo(0));
            Assert.That(critical, Is.EqualTo(0));

            vTimerSource.UpdateClock(1000);        // 1s elapsed.
            Assert.That(watchdog.Register("2", 30000, 45000), Is.True);
            Assert.That(warning, Is.EqualTo(0));
            Assert.That(critical, Is.EqualTo(0));

            Assert.That(watchdog.Unregister("1"), Is.True);

            vTimerSource.UpdateClock(29000);       // 30s elapsed. "1" is unregistered, so no warning.
            Assert.That(warning, Is.EqualTo(0));
            Assert.That(critical, Is.EqualTo(0));

            vTimerSource.UpdateClock(1000);        // 31s elapsed. "2" should trigger a warning (30sec).
            Assert.That(warning, Is.EqualTo(1));
            Assert.That(critical, Is.EqualTo(0));

            vTimerSource.UpdateClock(14000);       // 45s elapsed. "1" is unregistered, so no critical.
            Assert.That(warning, Is.EqualTo(1));
            Assert.That(critical, Is.EqualTo(0));

            vTimerSource.UpdateClock(1000);        // 46s elapsed. "2" should trigger a critical (45sec).
            Assert.That(warning, Is.EqualTo(1));
            Assert.That(critical, Is.EqualTo(1));
        }
    }
}
