namespace RJCP.Diagnostics.CrashExport.Xml
{
    using System.IO;
    using System.Xml;
    using NUnit.Framework;
#if NET45
    using System.Threading.Tasks;
#endif

    [TestFixture(Category = "CrashReporter.CrashExport")]
    public class XmlCrashDumperTest
    {
        [Test]
        public void DumpTestBlock()
        {
            using (ScratchPad scratch = Deploy.ScratchPad(ScratchOptions.CreateScratch | ScratchOptions.KeepCurrentDir)) {
                string dumpFileName = Path.Combine(scratch.Path, "Dump.xml");
                ICrashDumpFactory factory = new XmlCrashDumpFactory();
                using (ICrashDataDumpFile dump = factory.Create(dumpFileName)) {
                    ICrashDataExport block = new CrashData.TestBlock();
                    block.Dump(dump);
                    dump.Flush();
                }

                Assert.That(File.Exists(dumpFileName), Is.True);

                XmlDocument document = new XmlDocument();
                document.Load(dumpFileName);
                Assert.That(CheckDocument(document), Is.True);
            }
        }

        [Test]
        public void DumpTestBlockStream()
        {
            using (ScratchPad scratch = Deploy.ScratchPad(ScratchOptions.CreateScratch | ScratchOptions.KeepCurrentDir)) {
                ICrashDumpFactory factory = new XmlCrashDumpFactory();
                using (MemoryStream ms = new MemoryStream()) {
                    using (ICrashDataDumpFile dump = factory.Create(ms, scratch.Path)) {
                        ICrashDataExport block = new CrashData.TestBlock();
                        block.Dump(dump);
                        dump.Flush();
                    }
                    ms.Seek(0, SeekOrigin.Begin);

                    XmlDocument document = new XmlDocument();
                    document.Load(ms);
                    Assert.That(CheckDocument(document), Is.True);
                }
            }
        }

        private bool CheckDocument(XmlDocument document)
        {
            XmlNode rownode = document.SelectSingleNode("/DiagnosticDump/TestBlock/item");
            Assert.That(rownode.Attributes["Property"].Value, Is.EqualTo("TestProperty"));
            Assert.That(rownode.Attributes["Value"].Value, Is.EqualTo("TestValue"));
            return true;
        }

#if NET45
        [Test]
        public async Task DumpTestBlockAsync()
        {
            using (ScratchPad scratch = Deploy.ScratchPad(ScratchOptions.CreateScratch | ScratchOptions.KeepCurrentDir)) {
                string dumpFileName = Path.Combine(scratch.Path, "Dump.xml");
                ICrashDumpFactory factory = new XmlCrashDumpFactory();
                using (ICrashDataDumpFile dump = await factory.CreateAsync(dumpFileName)) {
                    ICrashDataExport block = new CrashData.TestBlock();
                    await block.DumpAsync(dump);
                    await dump.FlushAsync();
                }

                Assert.That(File.Exists(dumpFileName), Is.True);

                XmlDocument document = new XmlDocument();
                document.Load(dumpFileName);
                Assert.That(CheckDocument(document), Is.True);
            }
        }

        [Test]
        public async Task DumpTestBlockStreamAsync()
        {
            using (ScratchPad scratch = Deploy.ScratchPad(ScratchOptions.CreateScratch | ScratchOptions.KeepCurrentDir)) {
                ICrashDumpFactory factory = new XmlCrashDumpFactory();
                using (MemoryStream ms = new MemoryStream()) {
                    using (ICrashDataDumpFile dump = await factory.CreateAsync(ms, scratch.Path)) {
                        ICrashDataExport block = new CrashData.TestBlock();
                        await block.DumpAsync(dump);
                        await dump.FlushAsync();
                    }
                    ms.Seek(0, SeekOrigin.Begin);

                    XmlDocument document = new XmlDocument();
                    document.Load(ms);
                    Assert.That(CheckDocument(document), Is.True);
                }
            }
        }
#endif
    }
}
