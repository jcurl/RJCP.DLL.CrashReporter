namespace RJCP.Diagnostics.Watchdog.Timer
{
    using System;
    using System.Threading;
    using NUnit.Framework;

    [TestFixture]
    public class TimerListTest
    {
        [TestCase]
        public void Initialize()
        {
            TimerListAccessor timerList = new TimerListAccessor(new VirtualTimerSource(0));
            Assert.That(timerList.Count, Is.EqualTo(0));
            Assert.That(timerList.Active, Is.EqualTo(0));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(Timeout.Infinite));
        }

        [TestCase]
        public void InitializeNullTimerSource()
        {
            Assert.That(() => { _ = new TimerListAccessor(null); }, Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void AddNullItem()
        {
            TimerListAccessor timerList = new TimerListAccessor(new VirtualTimerSource(0));
            Assert.That(() => { timerList.Add(null, 1); }, Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void AddEnabledItem()
        {
            TimerListAccessor timerList = new TimerListAccessor(new VirtualTimerSource(0));
            timerList.Add("test", 1);
            Assert.That(timerList.Count, Is.EqualTo(1));
            Assert.That(timerList.Active, Is.EqualTo(1));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(1));  // Timeout in 1ms.
        }

        [TestCase(0, TestName = "AddEnabledItemWithRollover_0ms")]
        [TestCase(1, TestName = "AddEnabledItemWithRollover_1ms")]
        [TestCase(2, TestName = "AddEnabledItemWithRollover_2ms")]
        public void AddEnabledItemWithRollover(int timeout)
        {
            TimerListAccessor timerList = new TimerListAccessor(new VirtualTimerSource(-1));
            timerList.Add("test", timeout);
            Assert.That(timerList.Count, Is.EqualTo(1));
            Assert.That(timerList.Active, Is.EqualTo(1));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(timeout));
        }

        [TestCase]
        public void AddDisabledItem()
        {
            TimerListAccessor timerList = new TimerListAccessor(new VirtualTimerSource(0));
            timerList.Add("test", Timeout.Infinite);
            Assert.That(timerList.Count, Is.EqualTo(1));
            Assert.That(timerList.Active, Is.EqualTo(0));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(Timeout.Infinite));
        }

        [TestCase]
        public void AddItemsOrdered()
        {
            TimerListAccessor timerList = new TimerListAccessor(new VirtualTimerSource(-7));
            timerList.Add("1", 5);
            timerList.Add("2", 10);
            timerList.Add("3", 15);
            Assert.That(timerList.Count, Is.EqualTo(3));
            Assert.That(timerList.Active, Is.EqualTo(3));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(5));
        }

        [TestCase]
        public void AddItemsUnordered()
        {
            TimerListAccessor timerList = new TimerListAccessor(new VirtualTimerSource(-7));
            timerList.Add("3", 15);
            timerList.Add("2", 10);
            timerList.Add("1", 5);
            Assert.That(timerList.Count, Is.EqualTo(3));
            Assert.That(timerList.Active, Is.EqualTo(3));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(5));
        }

        [TestCase]
        public void AddDuplicateItemExpired()
        {
            TimerListAccessor timerList = new TimerListAccessor(new VirtualTimerSource(0));
            timerList.Add("test", Timeout.Infinite);
            Assert.That(() => { timerList.Add("test", Timeout.Infinite); }, Throws.TypeOf<ArgumentException>());
            Assert.That(timerList.Count, Is.EqualTo(1));
            Assert.That(timerList.Active, Is.EqualTo(0));
        }

        [TestCase]
        public void AddDuplicateItemUnexpired()
        {
            TimerListAccessor timerList = new TimerListAccessor(new VirtualTimerSource(0));
            timerList.Add("test", 2);
            Assert.That(() => { timerList.Add("test", 2); }, Throws.TypeOf<ArgumentException>());
            Assert.That(timerList.Count, Is.EqualTo(1));
            Assert.That(timerList.Active, Is.EqualTo(1));
        }

        [TestCase]
        public void AddDuplicateItemExpireMixed()
        {
            TimerListAccessor timerList = new TimerListAccessor(new VirtualTimerSource(0));
            timerList.Add("test", Timeout.Infinite);
            Assert.That(() => { timerList.Add("test", 2); }, Throws.TypeOf<ArgumentException>());
            Assert.That(timerList.Count, Is.EqualTo(1));
            Assert.That(timerList.Active, Is.EqualTo(0));
        }

        [TestCase]
        public void NextExpiryOffsetNegative()
        {
            VirtualTimerSource timerSource = new VirtualTimerSource(-2);
            TimerListAccessor timerList = new TimerListAccessor(timerSource);
            timerList.Add("1", 5);
            Assert.That(timerList.Count, Is.EqualTo(1));
            Assert.That(timerList.Active, Is.EqualTo(1));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(5));

            timerSource.UpdateClock(1);
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(4));

            timerSource.UpdateClock(1);
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(3));

            timerSource.UpdateClock(2);
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(1));

            timerSource.UpdateClock(1);
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(0));

            timerSource.UpdateClock(1);
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(-1));
            Assert.That(timerList.Count, Is.EqualTo(1));
            Assert.That(timerList.Active, Is.EqualTo(1));
        }

        [TestCase]
        public void ExpungeNothingExpired()
        {
            VirtualTimerSource timerSource = new VirtualTimerSource(-2);
            TimerListAccessor timerList = new TimerListAccessor(timerSource);
            timerList.Add("1", 5);
            Assert.That(timerList.Count, Is.EqualTo(1));
            Assert.That(timerList.Active, Is.EqualTo(1));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(5));

            Assert.That(timerList.ExpungeExpired(), Is.Empty);
            Assert.That(timerList.Count, Is.EqualTo(1));
            Assert.That(timerList.Active, Is.EqualTo(1));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(5));
        }

        [TestCase]
        public void ExpungeExpiredExactly()
        {
            VirtualTimerSource timerSource = new VirtualTimerSource(-2);
            TimerListAccessor timerList = new TimerListAccessor(timerSource);
            timerList.Add("1", 5);
            Assert.That(timerList.Count, Is.EqualTo(1));
            Assert.That(timerList.Active, Is.EqualTo(1));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(5));

            timerSource.UpdateClock(5);
            Assert.That(timerList.ExpungeExpired(), Is.EqualTo(new object[] { "1" }));
            Assert.That(timerList.Count, Is.EqualTo(1));
            Assert.That(timerList.Active, Is.EqualTo(0));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(Timeout.Infinite));
        }

        [TestCase]
        public void ExpungeExpiredOverdue()
        {
            VirtualTimerSource timerSource = new VirtualTimerSource(-2);
            TimerListAccessor timerList = new TimerListAccessor(timerSource);
            timerList.Add("1", 5);
            Assert.That(timerList.Count, Is.EqualTo(1));
            Assert.That(timerList.Active, Is.EqualTo(1));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(5));

            timerSource.UpdateClock(10);
            Assert.That(timerList.ExpungeExpired(), Is.EqualTo(new object[] { "1" }));
            Assert.That(timerList.Count, Is.EqualTo(1));
            Assert.That(timerList.Active, Is.EqualTo(0));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(Timeout.Infinite));
        }

        [TestCase]
        public void ExpungeIncrementally()
        {
            VirtualTimerSource timerSource = new VirtualTimerSource(-2);
            TimerListAccessor timerList = new TimerListAccessor(timerSource);
            timerList.Add("2", 10);
            timerList.Add("1", 5);
            Assert.That(timerList.Count, Is.EqualTo(2));
            Assert.That(timerList.Active, Is.EqualTo(2));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(5));

            timerSource.UpdateClock(6);
            Assert.That(timerList.ExpungeExpired(), Is.EqualTo(new object[] { "1" }));
            Assert.That(timerList.Count, Is.EqualTo(2));
            Assert.That(timerList.Active, Is.EqualTo(1));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(4));

            timerSource.UpdateClock(4);
            Assert.That(timerList.ExpungeExpired(), Is.EqualTo(new object[] { "2" }));
            Assert.That(timerList.Count, Is.EqualTo(2));
            Assert.That(timerList.Active, Is.EqualTo(0));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(Timeout.Infinite));
        }

        [TestCase]
        public void ExpungeMultipleEntries()
        {
            VirtualTimerSource timerSource = new VirtualTimerSource(-2);
            TimerListAccessor timerList = new TimerListAccessor(timerSource);
            timerList.Add("2", 10);
            timerList.Add("1", 5);
            Assert.That(timerList.Count, Is.EqualTo(2));
            Assert.That(timerList.Active, Is.EqualTo(2));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(5));

            timerSource.UpdateClock(11);
            Assert.That(timerList.ExpungeExpired(), Is.EqualTo(new object[] { "1", "2" }));
            Assert.That(timerList.Count, Is.EqualTo(2));
            Assert.That(timerList.Active, Is.EqualTo(0));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(Timeout.Infinite));
        }

        [TestCase]
        public void ExpungeEmptyList()
        {
            TimerListAccessor timerList = new TimerListAccessor(new VirtualTimerSource(-2));
            Assert.That(timerList.ExpungeExpired(), Is.Empty);
        }

        [TestCase]
        public void ResetTimer()
        {
            VirtualTimerSource timerSource = new VirtualTimerSource(-2);
            TimerListAccessor timerList = new TimerListAccessor(timerSource);
            timerList.Add("1", 5);
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(5));

            timerSource.UpdateClock(2);
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(3));

            timerList.Change("1", 5);
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(5));
        }

        [TestCase]
        public void ResetTimerMultipleItemsNoOrderChange()
        {
            VirtualTimerSource timerSource = new VirtualTimerSource(-2);
            TimerListAccessor timerList = new TimerListAccessor(timerSource);
            timerList.Add("1", 5);
            timerList.Add("2", 10);
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(5));

            timerSource.UpdateClock(2);
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(3));

            timerList.Change("1", 5);
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(5));
        }

        [TestCase]
        public void ResetTimerMultipleItemsOrderChange()
        {
            VirtualTimerSource timerSource = new VirtualTimerSource(-2);
            TimerListAccessor timerList = new TimerListAccessor(timerSource);
            timerList.Add("1", 5);
            timerList.Add("2", 6);
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(5));  // "1" next

            timerSource.UpdateClock(2);
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(3));  // "1" next

            timerList.Change("1", 5);
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(4));  // "2" next
        }

        [TestCase]
        public void ChangeNullItem()
        {
            TimerListAccessor timerList = new TimerListAccessor(new VirtualTimerSource(-2));
            Assert.That(() => { timerList.Change(null, 5); }, Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void ChangeNonExistentItem()
        {
            TimerListAccessor timerList = new TimerListAccessor(new VirtualTimerSource(-2));
            timerList.Add("1", 5);
            Assert.That(() => { timerList.Change("2", 5); }, Throws.TypeOf<ArgumentException>());
        }

        [TestCase]
        public void RemoveLastItem()
        {
            TimerListAccessor timerList = new TimerListAccessor(new VirtualTimerSource(-2));
            timerList.Add("1", 5);
            Assert.That(timerList.Contains("1"), Is.True);
            Assert.That(timerList.Count, Is.EqualTo(1));
            Assert.That(timerList.Active, Is.EqualTo(1));

            Assert.That(timerList.Remove("1"), Is.True);
            Assert.That(timerList.Contains("1"), Is.False);
            Assert.That(timerList.Count, Is.EqualTo(0));
            Assert.That(timerList.Active, Is.EqualTo(0));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(Timeout.Infinite));
        }

        [TestCase]
        public void RemoveFirstItem()
        {
            TimerListAccessor timerList = new TimerListAccessor(new VirtualTimerSource(-2));
            timerList.Add("1", 5);
            timerList.Add("2", 10);
            Assert.That(timerList.Contains("1"), Is.True);
            Assert.That(timerList.Contains("2"), Is.True);
            Assert.That(timerList.Count, Is.EqualTo(2));
            Assert.That(timerList.Active, Is.EqualTo(2));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(5));

            Assert.That(timerList.Remove("1"), Is.True);
            Assert.That(timerList.Contains("1"), Is.False);
            Assert.That(timerList.Contains("2"), Is.True);
            Assert.That(timerList.Count, Is.EqualTo(1));
            Assert.That(timerList.Active, Is.EqualTo(1));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(10));
        }

        [TestCase]
        public void RemoveEndItem()
        {
            TimerListAccessor timerList = new TimerListAccessor(new VirtualTimerSource(-2));
            timerList.Add("1", 5);
            timerList.Add("2", 10);
            Assert.That(timerList.Contains("1"), Is.True);
            Assert.That(timerList.Contains("2"), Is.True);
            Assert.That(timerList.Count, Is.EqualTo(2));
            Assert.That(timerList.Active, Is.EqualTo(2));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(5));

            Assert.That(timerList.Remove("2"), Is.True);
            Assert.That(timerList.Contains("1"), Is.True);
            Assert.That(timerList.Contains("2"), Is.False);
            Assert.That(timerList.Count, Is.EqualTo(1));
            Assert.That(timerList.Active, Is.EqualTo(1));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(5));
        }

        [TestCase]
        public void RemoveNullItem()
        {
            TimerListAccessor timerList = new TimerListAccessor(new VirtualTimerSource(-2));
            timerList.Add("1", 5);
            timerList.Add("2", 10);

            Assert.That(() => { timerList.Remove(null); }, Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void RemoveNonExistentItem()
        {
            TimerListAccessor timerList = new TimerListAccessor(new VirtualTimerSource(-2));
            timerList.Add("1", 5);
            timerList.Add("2", 10);

            Assert.That(timerList.Remove("3"), Is.False);
        }

        [TestCase]
        public void ClearActiveItems()
        {
            TimerListAccessor timerList = new TimerListAccessor(new VirtualTimerSource(-2));
            timerList.Add("1", 5);
            timerList.Add("2", 10);
            Assert.That(timerList.Count, Is.EqualTo(2));
            Assert.That(timerList.Active, Is.EqualTo(2));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(5));

            timerList.Clear();
            Assert.That(timerList.Count, Is.EqualTo(0));
            Assert.That(timerList.Active, Is.EqualTo(0));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(Timeout.Infinite));
        }

        [TestCase]
        public void ClearInactiveItems()
        {
            TimerListAccessor timerList = new TimerListAccessor(new VirtualTimerSource(-2));
            timerList.Add("1", Timeout.Infinite);
            timerList.Add("2", Timeout.Infinite);
            Assert.That(timerList.Count, Is.EqualTo(2));
            Assert.That(timerList.Active, Is.EqualTo(0));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(Timeout.Infinite));

            timerList.Clear();
            Assert.That(timerList.Count, Is.EqualTo(0));
            Assert.That(timerList.Active, Is.EqualTo(0));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(Timeout.Infinite));
        }

        [TestCase]
        public void ClearMixedItems()
        {
            TimerListAccessor timerList = new TimerListAccessor(new VirtualTimerSource(-2));
            timerList.Add("1", Timeout.Infinite);
            timerList.Add("2", 5);
            Assert.That(timerList.Count, Is.EqualTo(2));
            Assert.That(timerList.Active, Is.EqualTo(1));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(5));

            timerList.Clear();
            Assert.That(timerList.Count, Is.EqualTo(0));
            Assert.That(timerList.Active, Is.EqualTo(0));
            Assert.That(timerList.NextExpiryOffset(), Is.EqualTo(Timeout.Infinite));
        }
    }
}
