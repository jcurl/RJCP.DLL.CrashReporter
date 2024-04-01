namespace RJCP.Diagnostics.Crash.Export
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class MemoryCrashDataDumpFileAsyncTest
    {
        // The purpose of these tests is to ensure that the MemoryCrashDataDumpFile can be used to find common errors.

        [Test]
        public async Task GetTableAsync()
        {
            using (MemoryCrashDataDumpFile dump = new()) {
                using (IDumpTable table = await dump.DumpTableAsync("element", "item")) {
                    Assert.That(table, Is.Not.Null);
                    await table.FlushAsync();
                }
                await dump.FlushAsync();
                Assert.That(dump.Count, Is.EqualTo(1));
                Assert.That(dump["element"], Is.Not.Null);
                Assert.That(dump["element"].Table[0].TableName, Is.EqualTo("element"));
                Assert.That(dump["element"].Table[0].RowName, Is.EqualTo("item"));
            }
        }

        [TestCase(true, TestName = "GetTableAfterDispose")]
        [TestCase(false, TestName = "GetTableAfterDisposeNoFlush")]
        public async Task GetTableAfterDisposeAsync(bool flush)
        {
            MemoryCrashDataDumpFile dump = null;
            try {
                dump = new MemoryCrashDataDumpFile();
                using (IDumpTable table = await dump.DumpTableAsync("element", "item")) {
                    Assert.That(table, Is.Not.Null);
                    await table.FlushAsync();
                }
                Assert.That(dump.Count, Is.EqualTo(1));
                Assert.That(dump["element"], Is.Not.Null);
                if (flush) await dump.FlushAsync();
            } finally {
                if (dump is not null) dump.Dispose();
            }

            Assert.That(async () => { _ = await dump.DumpTableAsync("element2", "item"); }, Throws.TypeOf<ObjectDisposedException>());
        }

        [TestCase(true, TestName = "GetTableFlushAfterDisposeAsync")]
        [TestCase(false, TestName = "GetTableFlushAfterDisposeNoFlushAsync")]
        public async Task GetTableFlushAfterDisposeAsync(bool flush)
        {
            MemoryCrashDataDumpFile dump = null;
            try {
                dump = new MemoryCrashDataDumpFile();
                using (IDumpTable table = await dump.DumpTableAsync("element", "item")) {
                    Assert.That(table, Is.Not.Null);
                    await table.FlushAsync();
                }
                if (flush) await dump.FlushAsync();
            } finally {
                if (dump is not null) dump.Dispose();
            }

            Assert.That(async () => { await dump.FlushAsync(); }, Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public async Task GeTableAfterFlushAsync()
        {
            using (MemoryCrashDataDumpFile dump = new()) {
                using (IDumpTable table = await dump.DumpTableAsync("element", "item")) {
                    Assert.That(table, Is.Not.Null);
                    await table.FlushAsync();
                }
                await dump.FlushAsync();

                Assert.That(async () => { _ = await dump.DumpTableAsync("element2", "item"); }, Throws.TypeOf<InvalidOperationException>());
            }
        }

        [Test]
        public async Task GetDuplicateTableAsync()
        {
            using (MemoryCrashDataDumpFile dump = new()) {
                using (IDumpTable table = await dump.DumpTableAsync("element", "item")) {
                    Assert.That(table, Is.Not.Null);
                    await table.FlushAsync();
                }
                using (IDumpTable table = await dump.DumpTableAsync("element", "item")) {
                    Assert.That(table, Is.Not.Null);
                    await table.FlushAsync();
                }

                Assert.That(dump.Count, Is.EqualTo(1));
                Assert.That(dump["element"], Is.Not.Null);
                Assert.That(dump["element"].Table, Has.Count.EqualTo(2));
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(0));
                Assert.That(dump["element"].Table[1].Row.Count, Is.EqualTo(0));
                await dump.FlushAsync();
            }
        }

        [Test]
        public async Task GetNullTableNameAsync()
        {
            using (MemoryCrashDataDumpFile dump = new()) {
                Assert.That(async () => { _ = await dump.DumpTableAsync(null, "item"); }, Throws.TypeOf<ArgumentNullException>());
                await dump.FlushAsync();
            }
        }

        [Test]
        public async Task GetNullRowNameAsync()
        {
            using (MemoryCrashDataDumpFile dump = new()) {
                Assert.That(async () => { _ = await dump.DumpTableAsync("element", null); }, Throws.TypeOf<ArgumentNullException>());
                await dump.FlushAsync();
            }
        }

        [Test]
        public async Task FlushDumpTwiceAsync()
        {
            using (MemoryCrashDataDumpFile dump = new()) {
                await dump.FlushAsync();
                Assert.That(async () => { await dump.FlushAsync(); }, Throws.TypeOf<InvalidOperationException>());
            }
        }

        [Test]
        public async Task FlushTableTwiceAsync()
        {
            using (MemoryCrashDataDumpFile dump = new()) {
                using (IDumpTable table = await dump.DumpTableAsync("element", "item")) {
                    await table.FlushAsync();
                    Assert.That(async () => { await table.FlushAsync(); }, Throws.TypeOf<InvalidOperationException>());
                }
                await dump.FlushAsync();
            }
        }

        [Test]
        public async Task SetRowAsync()
        {
            Dictionary<string, string> row = new() { { "property", "value" } };

            using (MemoryCrashDataDumpFile dump = new()) {
                using (IDumpTable table = await dump.DumpTableAsync("element", "item")) {
                    Assert.That(table, Is.Not.Null);
                    await table.DumpRowAsync(row);
                    await table.FlushAsync();
                }
                Assert.That(dump.Count, Is.EqualTo(1));
                Assert.That(dump["element"], Is.Not.Null);
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(1));
                Assert.That(dump["element"].Table[0].Row[0].Field["property"], Is.EqualTo("value"));
                await dump.FlushAsync();
            }
        }

        [Test]
        public async Task SetRowNullAsync()
        {
            using (MemoryCrashDataDumpFile dump = new()) {
                using (IDumpTable table = await dump.DumpTableAsync("element", "item")) {
                    Assert.That(async () => { await table.DumpRowAsync((IDictionary<string, string>)null); }, Throws.TypeOf<ArgumentNullException>());
                    await table.FlushAsync();
                }
                Assert.That(dump.Count, Is.EqualTo(1));
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(0));
                dump.Flush();
            }
        }

        [Test]
        public async Task SetRowNullDumpRowAsync()
        {
            using (MemoryCrashDataDumpFile dump = new()) {
                using (IDumpTable table = await dump.DumpTableAsync("element", "item")) {
                    Assert.That(async () => { await table.DumpRowAsync((DumpRow)null); }, Throws.TypeOf<ArgumentNullException>());
                    await table.FlushAsync();
                }
                Assert.That(dump.Count, Is.EqualTo(1));
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(0));
                dump.Flush();
            }
        }

        [TestCase("", TestName = "SetFieldInvalidEmptyAsync")]
        [TestCase(" ", TestName = "SetFieldInvalidSpaceAsync")]
        [TestCase("abcd ", TestName = "SetFieldInvalidSpaceEndAsync")]
        [TestCase(" abcd ", TestName = "SetFieldInvalidSpaceStartEndAsync")]
        [TestCase(" abcd", TestName = "SetFieldInvalidSpaceStartAsync")]
        [TestCase("ab cd", TestName = "SetFieldInvalidSpaceMiddleAsync")]
        [TestCase("ab/cd", TestName = "SetFieldInvalidCharSlashAsync")]
        [TestCase("ab&cd", TestName = "SetFieldInvalidCharAmpersandAsync")]
        public async Task SetFieldInvalidAsync(string field)
        {
            Dictionary<string, string> row = new() { [field] = "value" };

            using (MemoryCrashDataDumpFile dump = new()) {
                using (IDumpTable table = await dump.DumpTableAsync("element", "item")) {
                    Assert.That(async () => { await table.DumpRowAsync(row); }, Throws.TypeOf<ArgumentException>());
                    await table.FlushAsync();
                }
                Assert.That(dump.Count, Is.EqualTo(1));
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(0));
                await dump.FlushAsync();
            }
        }

        [Test]
        public async Task SetRowAfterDisposeAsync()
        {
            Dictionary<string, string> row1 = new() { { "property", "value" } };
            Dictionary<string, string> row2 = new() { { "property2", "value2" } };

            using (MemoryCrashDataDumpFile dump = new()) {
                IDumpTable table = null;
                try {
                    table = await dump.DumpTableAsync("element", "item");
                    await table.DumpRowAsync(row1);
                    await table.FlushAsync();
                } finally {
                    if (table is not null) table.Dispose();
                }
                Assert.That(async () => { await table.DumpRowAsync(row2); }, Throws.TypeOf<ObjectDisposedException>());

                Assert.That(dump.Count, Is.EqualTo(1));
                Assert.That(dump["element"], Is.Not.Null);
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(1));
                Assert.That(dump["element"].Table[0].Row[0].Field["property"], Is.EqualTo("value"));
                dump.Flush();
            }
        }

        [Test]
        public async Task SetRowAfterFlushAsync()
        {
            Dictionary<string, string> row1 = new() { { "property", "value" } };
            Dictionary<string, string> row2 = new() { { "property2", "value2" } };

            using (MemoryCrashDataDumpFile dump = new()) {
                using (IDumpTable table = await dump.DumpTableAsync("element", "Item")) {
                    Assert.That(table, Is.Not.Null);
                    await table.DumpRowAsync(row1);
                    await table.FlushAsync();

                    Assert.That(async () => { await table.DumpRowAsync(row2); }, Throws.TypeOf<InvalidOperationException>());
                }
                await dump.FlushAsync();
            }
        }

        [Test]
        public async Task SetRowFlushAfterDisposeAsync()
        {
            Dictionary<string, string> row = new() { { "property", "value" } };

            using (MemoryCrashDataDumpFile dump = new()) {
                IDumpTable table = null;
                try {
                    table = await dump.DumpTableAsync("element", "item");
                    await table.DumpRowAsync(row);
                    await table.FlushAsync();
                } finally {
                    if (table is not null) table.Dispose();
                }
                Assert.That(async () => { await table.FlushAsync(); }, Throws.TypeOf<ObjectDisposedException>());
                await dump.FlushAsync();
            }
        }

        [TestCase(false, TestName = "DisposeDumpUndisposedTableAsync")]
        [TestCase(false, TestName = "DisposeDumpUndisposedFlushedTableAsync")]
        public async Task DisposeDumpUndisposedTableAsync(bool blockFlush)
        {
            MemoryCrashDataDumpFile dump = new();
            IDumpTable table = await dump.DumpTableAsync("element", "item");
            if (blockFlush) await table.FlushAsync();
            Assert.That(() => { dump.Dispose(); }, Throws.TypeOf<InvalidOperationException>());
            table.Dispose();
            dump.Dispose();
        }

        [Test]
        public async Task SynchronousDumpTableAsync()
        {
            using (MemoryCrashDataDumpFile dump = new()) {
                IDumpTable table = await dump.DumpTableAsync("element1", "item");
                Assert.That(async () => { _ = await dump.DumpTableAsync("element2", "item"); }, Throws.TypeOf<InvalidOperationException>());
                await table.FlushAsync();
                table.Dispose();
                await dump.FlushAsync();
                Assert.That(dump.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void AsynchronousDumpTable()
        {
            using (MemoryCrashDataDumpFile dump = new()) {
                IDumpTable table1 = dump.DumpTable("element1", "item");

                // Ensure that our test case will complain properly if we run two dumpers simultaneously. This should
                // not occur, and if it does, an exception should be raised.
                Assert.That(() => {
                    _ = dump.DumpTable("element2", "item");
                }, Throws.TypeOf<InvalidOperationException>());

                // The first table will succeed. Ensure it is still properly disposed so we don't get an exception when
                // disposing our table.
                table1.Flush();
                table1.Dispose();
                dump.Flush();
                Assert.That(dump.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task AsynchronousDumpTableAsync()
        {
            using (MemoryCrashDataDumpFile dump = new()) {
                Task<IDumpTable> table1Task = dump.DumpTableAsync("element1", "item");
                Task<IDumpTable> table2Task = dump.DumpTableAsync("element2", "item");

                // Ensure that our test case will complain properly if we run two dumpers simultaneously. This should
                // not occur, and if it does, an exception should be raised. Note, we don't have a race condition, as
                // the `MemoryCrashDataDumpFile.DumpTableAsync` will check for tables that haven't been disposed of yet
                // (which they can't be, that's done later in the test).
                await Assert.ThatAsync(async () => {
                    await Task.WhenAll(table1Task, table2Task);
                }, Throws.TypeOf<InvalidOperationException>());

                // One will succeed. Ensure it is still properly disposed so we don't get an exception when disposing
                // our table.
                if (!table1Task.IsFaulted) {
                    IDumpTable table1 = table1Task.Result;
                    await table1.FlushAsync();
                    table1.Dispose();
                }
                if (!table2Task.IsFaulted) {
                    IDumpTable table2 = table2Task.Result;
                    await table2.FlushAsync();
                    table2.Dispose();
                }
                await dump.FlushAsync();
            }
        }

        [Test]
        public async Task SetTableHeaderAsync()
        {
            Dictionary<string, string> row = new() {
                { "property", "value" },
                { "property2", "value2" }
            };

            using (MemoryCrashDataDumpFile dump = new()) {
                using (IDumpTable table = await dump.DumpTableAsync("element", "Item")) {
                    await table.DumpHeaderAsync(row.Keys);
                    await table.DumpRowAsync(row);
                    await table.FlushAsync();
                }
                await dump.FlushAsync();
                Assert.That(dump["element"], Is.Not.Null);
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(1));
                Assert.That(dump["element"].Table[0].Row[0].Field, Has.Count.EqualTo(2));
                Assert.That(dump["element"].Table[0].Row[0].Field["property"], Is.EqualTo("value"));
                Assert.That(dump["element"].Table[0].Row[0].Field["property2"], Is.EqualTo("value2"));
            }
        }

        [Test]
        public async Task SetTableHeaderAfterRowAsync()
        {
            Dictionary<string, string> row = new() {
                { "property", "value" },
                { "property2", "value2" }
            };

            using (MemoryCrashDataDumpFile dump = new()) {
                using (IDumpTable table = await dump.DumpTableAsync("element", "Item")) {
                    await table.DumpRowAsync(row);
                    Assert.That(async () => { await table.DumpHeaderAsync(row.Keys); }, Throws.TypeOf<InvalidOperationException>());
                    await table.FlushAsync();
                }
                await dump.FlushAsync();
                Assert.That(dump["element"], Is.Not.Null);
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(1));
                Assert.That(dump["element"].Table[0].Row[0].Field, Has.Count.EqualTo(2));
                Assert.That(dump["element"].Table[0].Row[0].Field["property"], Is.EqualTo("value"));
                Assert.That(dump["element"].Table[0].Row[0].Field["property2"], Is.EqualTo("value2"));
            }
        }

        [Test]
        public async Task SetTableHeaderNullAsync()
        {
            using (MemoryCrashDataDumpFile dump = new()) {
                using (IDumpTable table = await dump.DumpTableAsync("element", "Item")) {
                    Assert.That(async () => { await table.DumpHeaderAsync((IEnumerable<string>)null); }, Throws.TypeOf<ArgumentNullException>());
                    await table.FlushAsync();
                }
                await dump.FlushAsync();
            }
        }

        [Test]
        public async Task SetTableHeaderNullDumpRowAsync()
        {
            using (MemoryCrashDataDumpFile dump = new()) {
                using (IDumpTable table = await dump.DumpTableAsync("element", "Item")) {
                    Assert.That(async () => { await table.DumpHeaderAsync((DumpRow)null); }, Throws.TypeOf<ArgumentNullException>());
                    await table.FlushAsync();
                }
                await dump.FlushAsync();
            }
        }

        [Test]
        public async Task SetTableHeaderEmptyAsync()
        {
            using (MemoryCrashDataDumpFile dump = new()) {
                using (IDumpTable table = await dump.DumpTableAsync("element", "Item")) {
                    Assert.That(async () => { await table.DumpHeaderAsync(new string[] { }); }, Throws.TypeOf<ArgumentException>());
                    await table.FlushAsync();
                }
                await dump.FlushAsync();
            }
        }

        [Test]
        public async Task SetTableRowMissingFieldAsync()
        {
            Dictionary<string, string> row = new() {
                { "property", "value" },
                { "property2", "value2" }
            };

            using (MemoryCrashDataDumpFile dump = new()) {
                using (IDumpTable table = await dump.DumpTableAsync("element", "Item")) {
                    await table.DumpHeaderAsync(new[] { "property", "property2", "extra" });
                    Assert.That(async () => { await table.DumpRowAsync(row); }, Throws.TypeOf<ArgumentException>());
                    await table.FlushAsync();
                }
                await dump.FlushAsync();
                Assert.That(dump["element"], Is.Not.Null);
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public async Task SetTableRowExtraFieldAsync()
        {
            Dictionary<string, string> row = new() {
                { "property", "value" },
                { "property2", "value2" }
            };

            using (MemoryCrashDataDumpFile dump = new()) {
                using (IDumpTable table = await dump.DumpTableAsync("element", "Item")) {
                    await table.DumpHeaderAsync(new[] { "property" });
                    Assert.That(async () => { await table.DumpRowAsync(row); }, Throws.TypeOf<ArgumentException>());
                    await table.FlushAsync();
                }
                await dump.FlushAsync();
                Assert.That(dump["element"], Is.Not.Null);
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public async Task TableNotFoundAsync()
        {
            Dictionary<string, string> row = new() {
                { "property", "value" },
                { "property2", "value2" }
            };

            using (MemoryCrashDataDumpFile dump = new()) {
                using (IDumpTable table = await dump.DumpTableAsync("element", "Item")) {
                    await table.DumpRowAsync(row);
                    await table.FlushAsync();
                }
                await dump.FlushAsync();
                Assert.That(dump["element"], Is.Not.Null);
                Assert.That(() => { _ = dump["foo"]; }, Throws.TypeOf<KeyNotFoundException>());
            }
        }

        [Test]
        public async Task SetHeaderAfterDisposeAsync()
        {
            using (MemoryCrashDataDumpFile dump = new()) {
                IDumpTable table = await dump.DumpTableAsync("element", "item");
                table.Dispose();
                Assert.That(async () => { await table.DumpHeaderAsync(new string[] { "element" }); }, Throws.TypeOf<ObjectDisposedException>());

                Assert.That(dump.Count, Is.EqualTo(1));
                await dump.FlushAsync();
            }
        }

        [TestCase("", TestName = "SetHeaderFieldInvalidEmptyAsync")]
        [TestCase(" ", TestName = "SetHeaderFieldInvalidSpaceAsync")]
        [TestCase("abcd ", TestName = "SetHeaderFieldInvalidSpaceEndAsync")]
        [TestCase(" abcd ", TestName = "SetHeaderFieldInvalidSpaceStartEndAsync")]
        [TestCase(" abcd", TestName = "SetHeaderFieldInvalidSpaceStartAsync")]
        [TestCase("ab cd", TestName = "SetHeaderFieldInvalidSpaceMiddleAsync")]
        [TestCase("ab/cd", TestName = "SetHeaderFieldInvalidCharSlashAsync")]
        [TestCase("ab&cd", TestName = "SetHeaderFieldInvalidCharAmpersandAsync")]
        public async Task SetHeaderFieldInvalidAsync(string field)
        {
            using (MemoryCrashDataDumpFile dump = new()) {
                using (IDumpTable table = await dump.DumpTableAsync("element", "item")) {
                    Assert.That(async () => { await table.DumpHeaderAsync(new string[] { field }); }, Throws.TypeOf<ArgumentException>());
                    await table.FlushAsync();
                }
                Assert.That(dump.Count, Is.EqualTo(1));
                Assert.That(dump["element"].Table[0].Row.Count, Is.EqualTo(0));
                await dump.FlushAsync();
            }
        }
    }
}
