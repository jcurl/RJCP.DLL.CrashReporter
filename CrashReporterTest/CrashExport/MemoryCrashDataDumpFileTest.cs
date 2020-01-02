namespace RJCP.Diagnostics.CrashExport
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;

    [TestFixture(Category = "CrashReporter.CrashExport")]
    public class MemoryCrashDataDumpFileTest
    {
        // The purpose of these tests is to ensure that the MemoryCrashDataDumpFile can be used to find common errors.

        // NOTE: Test cases here should probably have similar tests in MemoryCrashDataDumpFileAsyncTest

        [Test]
        public void Default()
        {
            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                Assert.That(dump.IsSynchronous, Is.True);
                Assert.That(dump.Count, Is.EqualTo(0));
                dump.Flush();
            }
        }

        [Test]
        public void GetDumpPath()
        {
            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                Assert.That(dump.Path, Is.Not.Null.Or.Empty);
                Assert.That(System.IO.Directory.Exists(dump.Path));
            }
        }

        [Test]
        public void GetTable()
        {
            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                using (IDumpTable table = dump.DumpTable("element", "item")) {
                    Assert.That(table, Is.Not.Null);
                    table.Flush();
                }
                dump.Flush();
                Assert.That(dump.Count, Is.EqualTo(1));
                Assert.That(dump["element"], Is.Not.Null);
                Assert.That(dump["element"].Table[0].TableName, Is.EqualTo("element"));
                Assert.That(dump["element"].Table[0].RowName, Is.EqualTo("item"));
            }
        }

        [TestCase(true, TestName = "GetTableAfterDispose")]
        [TestCase(false, TestName = "GetTableAfterDisposeNoFlush")]
        public void GetTableAfterDispose(bool flush)
        {
            MemoryCrashDataDumpFile dump = null;
            try {
                dump = new MemoryCrashDataDumpFile();
                using (IDumpTable table = dump.DumpTable("element", "item")) {
                    Assert.That(table, Is.Not.Null);
                    table.Flush();
                }
                Assert.That(dump.Count, Is.EqualTo(1));
                Assert.That(dump["element"], Is.Not.Null);
                if (flush) dump.Flush();
            } finally {
                if (dump != null) dump.Dispose();
            }

            Assert.That(() => { _ = dump.DumpTable("element2", "item"); }, Throws.TypeOf<ObjectDisposedException>());
        }

        [TestCase(true, TestName = "GetTableFlushAfterDispose")]
        [TestCase(false, TestName = "GetTableFlushAfterDisposeNoFlush")]
        public void GetTableFlushAfterDispose(bool flush)
        {
            MemoryCrashDataDumpFile dump = null;
            try {
                dump = new MemoryCrashDataDumpFile();
                using (IDumpTable table = dump.DumpTable("element", "item")) {
                    Assert.That(table, Is.Not.Null);
                    table.Flush();
                }
                if (flush) dump.Flush();
            } finally {
                if (dump != null) dump.Dispose();
            }

            Assert.That(() => { dump.Flush(); }, Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void GeTableAfterFlush()
        {
            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                using (IDumpTable table = dump.DumpTable("element", "item")) {
                    Assert.That(table, Is.Not.Null);
                    table.Flush();
                }
                dump.Flush();

                Assert.That(() => { _ = dump.DumpTable("element2", "item"); }, Throws.TypeOf<InvalidOperationException>());
            }
        }

        [Test]
        public void GetDuplicateTable()
        {
            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                using (IDumpTable table = dump.DumpTable("element", "item")) {
                    Assert.That(table, Is.Not.Null);
                    table.Flush();
                }
                using (IDumpTable table = dump.DumpTable("element", "item")) {
                    Assert.That(table, Is.Not.Null);
                    table.Flush();
                }

                Assert.That(dump.Count, Is.EqualTo(1));
                Assert.That(dump["element"], Is.Not.Null);
                Assert.That(dump["element"].Table.Count, Is.EqualTo(2));
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(0));
                Assert.That(dump["element"].Table[1].Row.Count, Is.EqualTo(0));
                dump.Flush();
            }
        }

        [Test]
        public void GetNullTableName()
        {
            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                Assert.That(() => { _ = dump.DumpTable(null, "item"); }, Throws.TypeOf<ArgumentNullException>());
                dump.Flush();
            }
        }

        [Test]
        public void GetNullRowName()
        {
            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                Assert.That(() => { _ = dump.DumpTable("element", null); }, Throws.TypeOf<ArgumentNullException>());
                dump.Flush();
            }
        }

        [Test]
        public void FlushDumpTwice()
        {
            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                dump.Flush();
                Assert.That(() => { dump.Flush(); }, Throws.TypeOf<InvalidOperationException>());
            }
        }

        [Test]
        public void FlushTableTwice()
        {
            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                using (IDumpTable table = dump.DumpTable("element", "item")) {
                    table.Flush();
                    Assert.That(() => { table.Flush(); }, Throws.TypeOf<InvalidOperationException>());
                }
                dump.Flush();
            }
        }

        [Test]
        public void SetRow()
        {
            Dictionary<string, string> row = new Dictionary<string, string> { { "property",  "value" } };

            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                using (IDumpTable table = dump.DumpTable("element", "item")) {
                    Assert.That(table, Is.Not.Null);
                    table.DumpRow(row);
                    table.Flush();
                }
                Assert.That(dump.Count, Is.EqualTo(1));
                Assert.That(dump["element"], Is.Not.Null);
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(1));
                Assert.That(dump["element"].Table[0].Row[0].Field["property"], Is.EqualTo("value"));
                dump.Flush();
            }
        }

        [Test]
        public void SetRowNull()
        {
            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                using (IDumpTable table = dump.DumpTable("element", "item")) {
                    Assert.That(() => { table.DumpRow((IDictionary<string, string>)null); }, Throws.TypeOf<ArgumentNullException>());
                    table.Flush();
                }
                Assert.That(dump.Count, Is.EqualTo(1));
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(0));
                dump.Flush();
            }
        }

        [Test]
        public void SetRowNullDumpRow()
        {
            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                using (IDumpTable table = dump.DumpTable("element", "item")) {
                    Assert.That(() => { table.DumpRow((DumpRow)null); }, Throws.TypeOf<ArgumentNullException>());
                    table.Flush();
                }
                Assert.That(dump.Count, Is.EqualTo(1));
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(0));
                dump.Flush();
            }
        }

        [TestCase("", TestName = "SetFieldInvalidEmpty")]
        [TestCase(" ", TestName = "SetFieldInvalidSpace")]
        [TestCase("abcd ", TestName = "SetFieldInvalidSpaceEnd")]
        [TestCase(" abcd ", TestName = "SetFieldInvalidSpaceStartEnd")]
        [TestCase(" abcd", TestName = "SetFieldInvalidSpaceStart")]
        [TestCase("ab cd", TestName = "SetFieldInvalidSpaceMiddle")]
        [TestCase("ab/cd", TestName = "SetFieldInvalidCharSlash")]
        [TestCase("ab&cd", TestName = "SetFieldInvalidCharAmpersand")]
        public void SetFieldInvalid(string field)
        {
            Dictionary<string, string> row = new Dictionary<string, string> { [field] = "value" };

            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                using (IDumpTable table = dump.DumpTable("element", "item")) {
                    Assert.That(() => { table.DumpRow(row); }, Throws.TypeOf<ArgumentException>());
                    table.Flush();
                }
                Assert.That(dump.Count, Is.EqualTo(1));
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(0));
                dump.Flush();
            }
        }

        [Test]
        public void SetRowAfterDispose()
        {
            Dictionary<string, string> row1 = new Dictionary<string, string> { { "property",  "value" } };
            Dictionary<string, string> row2 = new Dictionary<string, string> { { "property2",  "value2" } };

            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                IDumpTable table = null;
                try {
                    table = dump.DumpTable("element", "item");
                    table.DumpRow(row1);
                    table.Flush();
                } finally {
                    if (table != null) table.Dispose();
                }
                Assert.That(() => { table.DumpRow(row2); }, Throws.TypeOf<ObjectDisposedException>());

                Assert.That(dump.Count, Is.EqualTo(1));
                Assert.That(dump["element"], Is.Not.Null);
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(1));
                Assert.That(dump["element"].Table[0].Row[0].Field["property"], Is.EqualTo("value"));
                dump.Flush();
            }
        }

        [Test]
        public void SetRowAfterFlush()
        {
            Dictionary<string, string> row1 = new Dictionary<string, string> { { "property", "value" } };
            Dictionary<string, string> row2 = new Dictionary<string, string> { { "property2", "value2" } };

            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                using (IDumpTable table = dump.DumpTable("element", "Item")) {
                    Assert.That(table, Is.Not.Null);
                    table.DumpRow(row1);
                    table.Flush();

                    Assert.That(() => { table.DumpRow(row2); }, Throws.TypeOf<InvalidOperationException>());
                }
                dump.Flush();
            }
        }

        [Test]
        public void SetRowFlushAfterDispose()
        {
            Dictionary<string, string> row = new Dictionary<string, string> { { "property", "value" } };

            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                IDumpTable table = null;
                try {
                    table = dump.DumpTable("element", "item");
                    table.DumpRow(row);
                    table.Flush();
                } finally {
                    if (table != null) table.Dispose();
                }
                Assert.That(() => { table.Flush(); }, Throws.TypeOf<ObjectDisposedException>());
                dump.Flush();
            }
        }

        [TestCase(false, TestName = "DisposeDumpUndisposedTable")]
        [TestCase(false, TestName = "DisposeDumpUndisposedFlushedTable")]
        public void DisposeDumpUndisposedTable(bool blockFlush)
        {
            MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile();
            IDumpTable table = dump.DumpTable("element", "item");
            if (blockFlush) table.Flush();
            Assert.That(() => { dump.Dispose(); }, Throws.TypeOf<InvalidOperationException>());
            table.Dispose();
            dump.Dispose();
        }

        [Test]
        public void SynchronousDumpTable()
        {
            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                IDumpTable table = dump.DumpTable("element1", "item");
                Assert.That(() => { _ = dump.DumpTable("element2", "item"); }, Throws.TypeOf<InvalidOperationException>());
                table.Flush();
                table.Dispose();
                dump.Flush();
                Assert.That(dump.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void AsynchronousDumpTable()
        {
            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                dump.IsSynchronous = false;
                IDumpTable table1 = dump.DumpTable("element1", "item");
                IDumpTable table2 = dump.DumpTable("element2", "item");
                table1.Flush();
                table1.Dispose();
                table2.Flush();
                table2.Dispose();
                dump.Flush();
                Assert.That(dump.Count, Is.EqualTo(2));
            }
        }

        [Test]
        public void SetTableHeader()
        {
            Dictionary<string, string> row = new Dictionary<string, string> {
                { "property", "value" },
                { "property2", "value2" }
            };

            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                using (IDumpTable table = dump.DumpTable("element", "Item")) {
                    table.DumpHeader(row.Keys);
                    table.DumpRow(row);
                    table.Flush();
                }
                dump.Flush();
                Assert.That(dump["element"], Is.Not.Null);
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(1));
                Assert.That(dump["element"].Table[0].Row[0].Field.Count, Is.EqualTo(2));
                Assert.That(dump["element"].Table[0].Row[0].Field["property"], Is.EqualTo("value"));
                Assert.That(dump["element"].Table[0].Row[0].Field["property2"], Is.EqualTo("value2"));
            }
        }

        [Test]
        public void SetTableHeaderAfterRow()
        {
            Dictionary<string, string> row = new Dictionary<string, string> {
                { "property", "value" },
                { "property2", "value2" }
            };

            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                using (IDumpTable table = dump.DumpTable("element", "Item")) {
                    table.DumpRow(row);
                    Assert.That(() => { table.DumpHeader(row.Keys); }, Throws.TypeOf<InvalidOperationException>());
                    table.Flush();
                }
                dump.Flush();
                Assert.That(dump["element"], Is.Not.Null);
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(1));
                Assert.That(dump["element"].Table[0].Row[0].Field.Count, Is.EqualTo(2));
                Assert.That(dump["element"].Table[0].Row[0].Field["property"], Is.EqualTo("value"));
                Assert.That(dump["element"].Table[0].Row[0].Field["property2"], Is.EqualTo("value2"));
            }
        }

        [Test]
        public void SetTableHeaderNull()
        {
            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                using (IDumpTable table = dump.DumpTable("element", "Item")) {
                    Assert.That(() => { table.DumpHeader((IEnumerable<string>)null); }, Throws.TypeOf<ArgumentNullException>());
                    table.Flush();
                }
                dump.Flush();
            }
        }

        [Test]
        public void SetTableHeaderNullDumpRow()
        {
            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                using (IDumpTable table = dump.DumpTable("element", "Item")) {
                    Assert.That(() => { table.DumpHeader((DumpRow)null); }, Throws.TypeOf<ArgumentNullException>());
                    table.Flush();
                }
                dump.Flush();
            }
        }

        [Test]
        public void SetTableHeaderEmpty()
        {
            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                using (IDumpTable table = dump.DumpTable("element", "Item")) {
                    Assert.That(() => { table.DumpHeader(new string[] { }); }, Throws.TypeOf<ArgumentException>());
                    table.Flush();
                }
                dump.Flush();
            }
        }

        [Test]
        public void SetTableRowMissingField()
        {
            Dictionary<string, string> row = new Dictionary<string, string> {
                { "property", "value" },
                { "property2", "value2" }
            };

            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                using (IDumpTable table = dump.DumpTable("element", "Item")) {
                    table.DumpHeader(new[] { "property", "property2", "extra" });
                    Assert.That(() => { table.DumpRow(row); }, Throws.TypeOf<ArgumentException>());
                    table.Flush();
                }
                dump.Flush();
                Assert.That(dump["element"], Is.Not.Null);
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void SetTableRowExtraField()
        {
            Dictionary<string, string> row = new Dictionary<string, string> {
                { "property", "value" },
                { "property2", "value2" }
            };

            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                using (IDumpTable table = dump.DumpTable("element", "Item")) {
                    table.DumpHeader(new[] { "property" });
                    Assert.That(() => { table.DumpRow(row); }, Throws.TypeOf<ArgumentException>());
                    table.Flush();
                }
                dump.Flush();
                Assert.That(dump["element"], Is.Not.Null);
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void TableNotFound()
        {
            Dictionary<string, string> row = new Dictionary<string, string> {
                { "property", "value" },
                { "property2", "value2" }
            };

            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                using (IDumpTable table = dump.DumpTable("element", "Item")) {
                    table.DumpRow(row);
                    table.Flush();
                }
                dump.Flush();
                Assert.That(dump["element"], Is.Not.Null);
                Assert.That(() => { _ = dump["foo"]; }, Throws.TypeOf<KeyNotFoundException>());
            }
        }

        [Test]
        public void SetHeaderAfterDispose()
        {
            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                IDumpTable table = dump.DumpTable("element", "item");
                table.Dispose();
                Assert.That(() => { table.DumpHeader(new string[] { "element" }); }, Throws.TypeOf<ObjectDisposedException>());

                Assert.That(dump.Count, Is.EqualTo(1));
                dump.Flush();
            }
        }

        [TestCase("", TestName = "SetHeaderFieldInvalidEmpty")]
        [TestCase(" ", TestName = "SetHeaderFieldInvalidSpace")]
        [TestCase("abcd ", TestName = "SetHeaderFieldInvalidSpaceEnd")]
        [TestCase(" abcd ", TestName = "SetHeaderFieldInvalidSpaceStartEnd")]
        [TestCase(" abcd", TestName = "SetHeaderFieldInvalidSpaceStart")]
        [TestCase("ab cd", TestName = "SetHeaderFieldInvalidSpaceMiddle")]
        [TestCase("ab/cd", TestName = "SetHeaderFieldInvalidCharSlash")]
        [TestCase("ab&cd", TestName = "SetHeaderFieldInvalidCharAmpersand")]
        public void SetHeaderFieldInvalid(string field)
        {
            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                using (IDumpTable table = dump.DumpTable("element", "item")) {
                    Assert.That(() => { table.DumpHeader(new string[] { field }); }, Throws.TypeOf<ArgumentException>());
                    table.Flush();
                }
                Assert.That(dump.Count, Is.EqualTo(1));
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(0));
                dump.Flush();
            }
        }

        public void SetHeaderFieldDuplicate()
        {
            using (MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile()) {
                using (IDumpTable table = dump.DumpTable("element", "item")) {
                    Assert.That(() => { table.DumpHeader(new string[] { "field", "field" }); }, Throws.TypeOf<ArgumentException>());
                    table.Flush();
                }
                Assert.That(dump.Count, Is.EqualTo(1));
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(0));
                dump.Flush();
            }
        }
    }
}
