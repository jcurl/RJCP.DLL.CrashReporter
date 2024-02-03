namespace CrashReportApp
{
    using RJCP.Diagnostics.Trace;
#if NET6_0_OR_GREATER
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using RJCP.Diagnostics.Logging;
#endif

    public static class Log
    {
        public static LogSource App { get; private set; }

        static Log()
        {
#if NET6_0_OR_GREATER
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, false)
                .Build();

            ILoggerFactory factory = LoggerFactory.Create(builder => {
                builder
                    .AddConfiguration(config.GetSection("Logging"))
                    .AddConsole()
                    .AddSimplePrioMemoryLogger();
            });

            LogSource.SetLoggerFactory(factory);
#endif
            App = new LogSource("CrashReporterApp");
        }
    }
}
