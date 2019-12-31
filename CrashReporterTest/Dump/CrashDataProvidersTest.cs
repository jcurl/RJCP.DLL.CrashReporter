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
            Assert.That(Crash.Data.Providers.Count, Is.EqualTo(4));
            Assert.That(HasProviderType(typeof(CrashData.NetVersionDump)), Is.True);
            Assert.That(HasProviderType(typeof(CrashData.AssemblyDump)), Is.True);
            Assert.That(HasProviderType(typeof(CrashData.EnvironmentDump)), Is.True);
            Assert.That(HasProviderType(typeof(CrashData.NetworkDump)), Is.True);
        }

        private bool HasProviderType(Type provider)
        {
            return Crash.Data.Providers.Any((d) => d.GetType().Equals(provider));
        }
    }
}
