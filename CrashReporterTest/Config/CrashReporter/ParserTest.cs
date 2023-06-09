namespace RJCP.Diagnostics.Config.CrashReporter
{
    using System;
    using System.IO;
    using NUnit.Framework;

    [TestFixture]
    public class ParserTest
    {
        [Test]
        public void ParseEnvVarString()
        {
            Assert.That(ParserAccessor.ParseEnvVar("test"), Is.EqualTo("test"));
        }

        [Test]
        public void ParseEnvVarNull()
        {
            Assert.That(ParserAccessor.ParseEnvVar(null), Is.Empty);
        }

        [Test]
        public void ParseEnvCwd()
        {
            Assert.That(ParserAccessor.ParseEnvVar("${CWD}"), Is.EqualTo(Environment.CurrentDirectory));
        }

        [Test]
        public void ParseEnvAppDir()
        {
            string appDir = ParserAccessor.ParseEnvVar("${APPDIR}");
            Assert.That(Directory.Exists(appDir), Is.True);
        }

        [Test]
        public void ParseEnvHome()
        {
            string homeDir = ParserAccessor.ParseEnvVar("${HOME}");
            Assert.That(Directory.Exists(homeDir), Is.True);
        }

        [Test]
        public void ParseEnvLocalAppData()
        {
            string appData = ParserAccessor.ParseEnvVar("${LOCALAPPDATA}");
            Assert.That(Directory.Exists(appData), Is.True);
        }

        [Test]
        public void ParseEnvAppData()
        {
            string appData = ParserAccessor.ParseEnvVar("${APPDATA}");
            Assert.That(Directory.Exists(appData), Is.True);
        }

        [Test]
        public void ParseEnvVar()
        {
            Environment.SetEnvironmentVariable("FOO", "foo");
            Assert.That(ParserAccessor.ParseEnvVar("${FOO}"), Is.EqualTo("foo"));
        }

        [Test]
        public void ParseEnvVarNone()
        {
            Assert.That(ParserAccessor.ParseEnvVar("${XXXNOEXIST}"), Is.Empty);
        }

        [Test]
        public void ParseCrashDumpDir()
        {
            string dumpDir = ParserAccessor.ParseEnvVar("${LOCALAPPDATA}/CrashDump");
            string expectedDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CrashDump");
            Assert.That(dumpDir, Is.EqualTo(expectedDir));
        }
    }
}
