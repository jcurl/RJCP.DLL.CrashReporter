namespace RJCP.Diagnostics.Crash.Export
{
    using System.IO;
#if !NET40_LEGACY
    using System.Threading.Tasks;
#endif

    public class MemoryCrashDumpFactory : ICrashDumpFactory
    {
        public string FileName { get { return string.Empty; } }

        public ICrashDataDumpFile Create(string fileName)
        {
            return new MemoryCrashDataDumpFile();
        }

        public ICrashDataDumpFile Create(Stream stream, string path)
        {
            return new MemoryCrashDataDumpFile();
        }

#if !NET40_LEGACY
        public Task<ICrashDataDumpFile> CreateAsync(string fileName)
        {
            return Task.FromResult<ICrashDataDumpFile>(new MemoryCrashDataDumpFile());
        }

        public Task<ICrashDataDumpFile> CreateAsync(Stream stream, string path)
        {
            return Task.FromResult<ICrashDataDumpFile>(new MemoryCrashDataDumpFile());
        }
#endif
    }
}
