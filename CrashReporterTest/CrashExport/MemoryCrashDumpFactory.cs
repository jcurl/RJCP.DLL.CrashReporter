namespace RJCP.Diagnostics.CrashExport
{
    using System.IO;
#if NET45_OR_GREATER || NETCOREAPP
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
            MemoryCrashDataDumpFile dump = new MemoryCrashDataDumpFile {
                IsSynchronous = IsSynchronous
            };
            return dump;
        }

#if NET45_OR_GREATER || NETCOREAPP
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
