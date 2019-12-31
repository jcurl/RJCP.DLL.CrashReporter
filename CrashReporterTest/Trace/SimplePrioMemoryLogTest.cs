namespace RJCP.Diagnostics.Trace
{
    using System.Diagnostics;
    using System.Linq;
    using NUnit.Framework;

    [TestFixture(Category = "CrashReporter.Trace")]
    public class SimplePrioMemoryLogTest
    {
        [Test]
        public void DefaultSettings()
        {
            SimplePrioMemoryLog log = new SimplePrioMemoryLog();
            Assert.That(log.Critical + log.Error + log.Warning + log.Info + log.Verbose, Is.LessThanOrEqualTo(log.Total));
        }

        [Test]
        public void AddEntryOrdered1()
        {
            SimplePrioMemoryLog log = new SimplePrioMemoryLog {
                new LogEntry(TraceEventType.Critical, 0, "Critical", 0),
                new LogEntry(TraceEventType.Error, 0, "Error", 1),
                new LogEntry(TraceEventType.Warning, 0, "Warning", 2),
                new LogEntry(TraceEventType.Information, 0, "Information", 3),
                new LogEntry(TraceEventType.Verbose, 0, "Verbose", 4),
                new LogEntry(TraceEventType.Suspend, 0, "Suspend", 5),
                new LogEntry(TraceEventType.Resume, 0, "Resume", 6)
            };

            Assert.That(log.Count, Is.EqualTo(7));

            var source = from entry in log select entry.Message;
            Assert.That(source, Is.EqualTo(new string[] { "Critical", "Error", "Warning", "Information", "Verbose", "Suspend", "Resume" }));
        }

        [Test]
        public void AddEntryOrdered2()
        {
            SimplePrioMemoryLog log = new SimplePrioMemoryLog {
                new LogEntry(TraceEventType.Critical, 6, "Critical", 6),
                new LogEntry(TraceEventType.Error, 5, "Error", 5),
                new LogEntry(TraceEventType.Warning, 4, "Warning", 4),
                new LogEntry(TraceEventType.Information, 3, "Information", 3),
                new LogEntry(TraceEventType.Verbose, 2, "Verbose", 2),
                new LogEntry(TraceEventType.Suspend, 1, "Suspend", 1),
                new LogEntry(TraceEventType.Resume, 0, "Resume", 0)
            };

            Assert.That(log.Count, Is.EqualTo(7));

            // The order might be different to what might at first glance be expected. Ordering is sorted by clock
            // across priority. If the clock for a given priority entry is reversed, it may delay printing of other
            // objects by clock. So output is not strictly ordered by clock if ordering is exchanged.
            var source = from entry in log select entry.Message;
            Assert.That(source, Is.EqualTo(new string[] { "Suspend", "Resume", "Verbose", "Information", "Warning", "Error", "Critical" }));
        }

        [Test]
        public void DiscardLowerPriority()
        {
            SimplePrioMemoryLog log = new SimplePrioMemoryLog {
                Critical = 3,
                Error = 3,
                Warning = 3,
                Info = 3,
                Verbose = 3,
                Other = 3,
                Total = 21
            };

            for (int clock = 0; clock < 21; clock++) {
                log.Add(new LogEntry(TraceEventType.Information, 0, "Info", clock));
            }
            Assert.That(log.Count, Is.EqualTo(21));

            // Add a critical, there should be one less Info.
            log.Add(new LogEntry(TraceEventType.Critical, 0, "Critical", 21));
            Assert.That(log.Count, Is.EqualTo(21));
            Assert.That(log.Count((entry) => entry.EventType == TraceEventType.Information), Is.EqualTo(20));
            Assert.That(log.Count((entry) => entry.EventType == TraceEventType.Critical), Is.EqualTo(1));

            // Add 17 Warnings, so all Info are dropped until a minimum of 3
            for (int clock = 22; clock < 39; clock++) {
                log.Add(new LogEntry(TraceEventType.Warning, 0, "Warning", clock));
            }
            Assert.That(log.Count, Is.EqualTo(21));
            Assert.That(log.Count((entry) => entry.EventType == TraceEventType.Information), Is.EqualTo(3));
            Assert.That(log.Count((entry) => entry.EventType == TraceEventType.Warning), Is.EqualTo(17));
            Assert.That(log.Count((entry) => entry.EventType == TraceEventType.Critical), Is.EqualTo(1));

            // Add a Info, the warnings should not be discarded
            log.Add(new LogEntry(TraceEventType.Information, 0, "Info", 39));
            Assert.That(log.Count, Is.EqualTo(21));
            Assert.That(log.Count((entry) => entry.EventType == TraceEventType.Information), Is.EqualTo(3));
            Assert.That(log.Count((entry) => entry.EventType == TraceEventType.Warning), Is.EqualTo(17));
            Assert.That(log.Count((entry) => entry.EventType == TraceEventType.Critical), Is.EqualTo(1));

            // Add a Verbose, and the next low prio should be removed (a Warning in preference to a Verbose, as there are
            // no Verbose and minimum is 3, and Warnings exceed the minimum).
            log.Add(new LogEntry(TraceEventType.Verbose, 0, "Verbose", 40));
            Assert.That(log.Count, Is.EqualTo(21));
            Assert.That(log.Count((entry) => entry.EventType == TraceEventType.Verbose), Is.EqualTo(1));
            Assert.That(log.Count((entry) => entry.EventType == TraceEventType.Information), Is.EqualTo(3));
            Assert.That(log.Count((entry) => entry.EventType == TraceEventType.Warning), Is.EqualTo(16));
            Assert.That(log.Count((entry) => entry.EventType == TraceEventType.Critical), Is.EqualTo(1));
        }
    }
}
