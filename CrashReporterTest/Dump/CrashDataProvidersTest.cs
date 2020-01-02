namespace RJCP.Diagnostics.Dump
{
    using System;
    using System.Linq;
    using NUnit.Framework;

    [TestFixture(Category = "CrashReporter.Dump")]
    public class CrashDataProvidersTest
    {
        [Test]
        public void DefaultListOfProviders()
        {
            // Listeners are dynamically added. We can't know if a TraceListener test was run before or after this test.
            Assert.That(Crash.Data.Providers.Count - Listeners(), Is.EqualTo(9));
            Assert.That(HasProviderType(typeof(CrashData.NetVersionDump)), Is.True);
            Assert.That(HasProviderType(typeof(CrashData.AssemblyDump)), Is.True);
            Assert.That(HasProviderType(typeof(CrashData.EnvironmentDump)), Is.True);
            Assert.That(HasProviderType(typeof(CrashData.NetworkDump)), Is.True);
            Assert.That(HasProviderType(typeof(CrashData.ThreadDump)), Is.True);
            Assert.That(HasProviderType(typeof(CrashData.OSDump)), Is.True);
            Assert.That(HasProviderType(typeof(CrashData.WinVerDump)), Is.True);
            Assert.That(HasProviderType(typeof(CrashData.ProcessDump)), Is.True);
            Assert.That(HasProviderType(typeof(CrashData.ModuleDump)), Is.True);
        }

        private bool HasProviderType(Type provider)
        {
            return Crash.Data.Providers.Any((d) => d.GetType().Equals(provider));
        }

        private int Listeners()
        {
            return Crash.Data.Providers.Count((d) => d.GetType().IsAssignableFrom(typeof(Trace.MemoryTraceListener)));
        }
    }
}
