namespace RJCP.Diagnostics.Dump
{
    using System;
    using System.Linq;
    using Crash.Dumpers;
    using NUnit.Framework;

    [TestFixture(Category = "CrashReporter.Dump")]
    public class CrashDataProvidersTest
    {
        [Test]
        public void DefaultListOfProviders()
        {
            // Listeners are dynamically added. We can't know if a TraceListener test was run before or after this test.
            Assert.That(CrashData.Instance.Providers.Count - Listeners(), Is.EqualTo(9));
            Assert.That(HasProviderType(typeof(NetVersionDump)), Is.True);
            Assert.That(HasProviderType(typeof(AssemblyDump)), Is.True);
            Assert.That(HasProviderType(typeof(EnvironmentDump)), Is.True);
            Assert.That(HasProviderType(typeof(NetworkDump)), Is.True);
            Assert.That(HasProviderType(typeof(ThreadDump)), Is.True);
            Assert.That(HasProviderType(typeof(OSDump)), Is.True);
            Assert.That(HasProviderType(typeof(WinVerDump)), Is.True);
            Assert.That(HasProviderType(typeof(ProcessDump)), Is.True);
            Assert.That(HasProviderType(typeof(ModuleDump)), Is.True);
        }

        private static bool HasProviderType(Type provider)
        {
            return CrashData.Instance.Providers.Any((d) => d.GetType().Equals(provider));
        }

        private static int Listeners()
        {
            return CrashData.Instance.Providers.Count((d) => d.GetType().IsAssignableFrom(typeof(Trace.MemoryTraceListener)));
        }
    }
}
