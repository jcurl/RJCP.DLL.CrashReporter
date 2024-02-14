namespace RJCP.Diagnostics.Crash.Export.Xml
{
    using System.IO;
    using System.Xml;
    using Crash.Dumpers;
    using NUnit.Framework;
    using RJCP.CodeQuality.NUnitExtensions;
#if !NET40_LEGACY
    using System.Threading.Tasks;
#endif

    [TestFixture]
    public class XmlCrashDumperTest
    {
        [Test]
        public void DumpTestBlock()
        {
            using (ScratchPad scratch = Deploy.ScratchPad(ScratchOptions.CreateScratch | ScratchOptions.KeepCurrentDir)) {
                string dumpFileName = Path.Combine(scratch.Path, "Dump.xml");
                ICrashDumpFactory factory = new XmlCrashDumpFactory();
                using (ICrashDataDumpFile dump = factory.Create(dumpFileName)) {
                    ICrashDataExport block = new TestBlock();
                    block.Dump(dump);
                    dump.Flush();
                }

                Assert.That(File.Exists(dumpFileName), Is.True);

                XmlDocument document = new();
                document.Load(dumpFileName);
                Assert.That(CheckDocument(document), Is.True);
            }
        }

        [Test]
        public void DumpTestBlockStream()
        {
            using (ScratchPad scratch = Deploy.ScratchPad(ScratchOptions.CreateScratch | ScratchOptions.KeepCurrentDir)) {
                ICrashDumpFactory factory = new XmlCrashDumpFactory();
                using (MemoryStream ms = new()) {
                    using (ICrashDataDumpFile dump = factory.Create(ms, scratch.Path)) {
                        ICrashDataExport block = new TestBlock();
                        block.Dump(dump);
                        dump.Flush();
                    }
                    ms.Seek(0, SeekOrigin.Begin);

                    XmlDocument document = new();
                    document.Load(ms);
                    Assert.That(CheckDocument(document), Is.True);
                }
            }
        }

        [TestCase("\x8", "[0x08]", TestName = "DumpWithInvalidCharacters(bs)")]
        [TestCase("\x0", "[0x00]", TestName = "DumpWithInvalidCharacters(nul)")]
        [TestCase("C\x8L\x8I\x8N\x8K\x8 \x8$P$G", "C[0x08]L[0x08]I[0x08]N[0x08]K[0x08] [0x08]$P$G", TestName = "DumpWithInvalidCharacters(PS)")]
        [TestCase("", "", TestName = "DumpWithInvalidCharacters(empty)")]
        [TestCase(null, "", TestName = "DumpWithInvalidCharacters(null)")]
        [TestCase("string", "string", TestName = "DumpWithInvalidCharacters(string)")]
        [TestCase("string\x0", "string[0x00]", TestName = "DumpWithInvalidCharacters(string nul)")]
        [TestCase("\x0string", "[0x00]string", TestName = "DumpWithInvalidCharacters(nul string)")]
        public void DumpWithInvalidCharacters(string input, string expected)
        {
            using (ScratchPad scratch = Deploy.ScratchPad(ScratchOptions.CreateScratch | ScratchOptions.KeepCurrentDir)) {
                ICrashDumpFactory factory = new XmlCrashDumpFactory();
                using (MemoryStream ms = new()) {
                    using (ICrashDataDumpFile dump = factory.Create(ms, scratch.Path)) {
                        TestBlock block = new();
                        block.AddEntry("TestKey", input);
                        block.Dump(dump);
                        dump.Flush();
                    }
                    ms.Seek(0, SeekOrigin.Begin);

                    XmlDocument document = new();
                    document.Load(ms);
                    Assert.That(CheckDocument(document), Is.True);

                    XmlNode rownode = document.SelectSingleNode("/DiagnosticDump/TestBlock/item");
                    Assert.That(rownode.Attributes["TestKey"].Value, Is.EqualTo(expected));
                }
            }
        }

        private static bool CheckDocument(XmlDocument document)
        {
            XmlNode rownode = document.SelectSingleNode("/DiagnosticDump/TestBlock/item");
            Assert.That(rownode.Attributes["Property"].Value, Is.EqualTo("TestProperty"));
            Assert.That(rownode.Attributes["Value"].Value, Is.EqualTo("TestValue"));
            return true;
        }

#if !NET40_LEGACY
        [Test]
        public async Task DumpTestBlockAsync()
        {
            using (ScratchPad scratch = Deploy.ScratchPad(ScratchOptions.CreateScratch | ScratchOptions.KeepCurrentDir)) {
                string dumpFileName = Path.Combine(scratch.Path, "Dump.xml");
                ICrashDumpFactory factory = new XmlCrashDumpFactory();
                using (ICrashDataDumpFile dump = await factory.CreateAsync(dumpFileName)) {
                    ICrashDataExport block = new TestBlock();
                    await block.DumpAsync(dump);
                    await dump.FlushAsync();
                }

                Assert.That(File.Exists(dumpFileName), Is.True);

                XmlDocument document = new();
                document.Load(dumpFileName);
                Assert.That(CheckDocument(document), Is.True);
            }
        }

        [Test]
        public async Task DumpTestBlockStreamAsync()
        {
            using (ScratchPad scratch = Deploy.ScratchPad(ScratchOptions.CreateScratch | ScratchOptions.KeepCurrentDir)) {
                ICrashDumpFactory factory = new XmlCrashDumpFactory();
                using (MemoryStream ms = new()) {
                    using (ICrashDataDumpFile dump = await factory.CreateAsync(ms, scratch.Path)) {
                        ICrashDataExport block = new TestBlock();
                        await block.DumpAsync(dump);
                        await dump.FlushAsync();
                    }
                    ms.Seek(0, SeekOrigin.Begin);

                    XmlDocument document = new();
                    document.Load(ms);
                    Assert.That(CheckDocument(document), Is.True);
                }
            }
        }
#endif
    }
}
