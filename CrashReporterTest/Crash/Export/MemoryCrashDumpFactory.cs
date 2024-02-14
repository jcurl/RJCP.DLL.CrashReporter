namespace RJCP.Diagnostics.Crash.Export
{
    using System.IO;
#if !NET40_LEGACY
    using System.Threading.Tasks;
#endif

    public class MemoryCrashDumpFactory : ICrashDumpFactory
    {
        public bool IsSynchronous { get; set; }

        public string FileName { get { return string.Empty; } }

        public ICrashDataDumpFile Create(string fileName)
        {
            return CreateDefault();
        }

        public ICrashDataDumpFile Create(Stream stream, string path)
        {
            return CreateDefault();
        }

        private ICrashDataDumpFile CreateDefault()
        {
            MemoryCrashDataDumpFile dump = new() {
                IsSynchronous = IsSynchronous
            };
            return dump;
        }

#if !NET40_LEGACY
        public Task<ICrashDataDumpFile> CreateAsync(string fileName)
        {
            return Task.FromResult(CreateDefault());
        }

        public Task<ICrashDataDumpFile> CreateAsync(Stream stream, string path)
        {
            return Task.FromResult(CreateDefault());
        }
#endif
    }
}
