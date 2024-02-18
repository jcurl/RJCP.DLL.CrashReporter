namespace RJCP.Diagnostics.Crash
{
    using System;
    using System.Linq;
    using System.Runtime.Versioning;
    using Dumpers;
    using NUnit.Framework;

    [TestFixture]
    public class CrashDataProvidersTest
    {
        [Test]
        [Platform("Win32NT")]
        [SupportedOSPlatform("windows")]
        public void DefaultListOfProvidersWindows()
        {
            // Listeners are dynamically added. We can't know if a TraceListener test was run before or after this test.
            Assert.That(CrashData.Instance.Providers.Count - Listeners(), Is.EqualTo(9));
            Assert.That(HasProviderType(NetVersionDumpAccessor.AccType.ReferencedType), Is.True);
            Assert.That(HasProviderType(AssemblyDumpAccessor.AccType.ReferencedType), Is.True);
            Assert.That(HasProviderType(EnvironmentDumpAccessor.AccType.ReferencedType), Is.True);
            Assert.That(HasProviderType(NetworkDumpAccessor.AccType.ReferencedType), Is.True);
            Assert.That(HasProviderType(ThreadDumpAccessor.AccType.ReferencedType), Is.True);
            Assert.That(HasProviderType(OSDumpAccessor.AccType.ReferencedType), Is.True);
            Assert.That(HasProviderType(WinVerDumpAccessor.AccType.ReferencedType), Is.True);
            Assert.That(HasProviderType(ProcessDumpAccessor.AccType.ReferencedType), Is.True);
            Assert.That(HasProviderType(ModuleDumpAccessor.AccType.ReferencedType), Is.True);
        }

        [Test]
        [Platform("Linux")]
        [SupportedOSPlatform("linux")]
        public void DefaultListOfProvidersLinux()
        {
            // Listeners are dynamically added. We can't know if a TraceListener test was run before or after this test.
            Assert.That(CrashData.Instance.Providers.Count - Listeners(), Is.EqualTo(8));
            Assert.That(HasProviderType(NetVersionDumpAccessor.AccType.ReferencedType), Is.True);
            Assert.That(HasProviderType(AssemblyDumpAccessor.AccType.ReferencedType), Is.True);
            Assert.That(HasProviderType(EnvironmentDumpAccessor.AccType.ReferencedType), Is.True);
            Assert.That(HasProviderType(NetworkDumpAccessor.AccType.ReferencedType), Is.True);
            Assert.That(HasProviderType(ThreadDumpAccessor.AccType.ReferencedType), Is.True);
            Assert.That(HasProviderType(OSDumpAccessor.AccType.ReferencedType), Is.True);
            Assert.That(HasProviderType(ProcessDumpAccessor.AccType.ReferencedType), Is.True);
            Assert.That(HasProviderType(ModuleDumpAccessor.AccType.ReferencedType), Is.True);
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
