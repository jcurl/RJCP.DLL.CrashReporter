namespace RJCP.Diagnostics.Logging
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Configuration;

    /// <summary>
    /// Memory Logger Extensions to configure for your applications.
    /// </summary>
    [CLSCompliant(false)]
    public static class MemoryLoggerExtensions
    {
        /// <summary>
        /// Adds the <see cref="SimplePrioMemoryLogProvider"/> for logging.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <returns>The <see cref="ILoggingBuilder"/> for fluent construction.</returns>
        public static ILoggingBuilder AddSimplePrioMemoryLogger(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider, SimplePrioMemoryLogProvider>());

            LoggerProviderOptions.RegisterProviderOptions
                <SimplePrioMemoryLogConfig, SimplePrioMemoryLogProvider>(builder.Services);
            return builder;
        }

        /// <summary>
        /// Adds the simple prio memory logger.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="configure">A delegate to configure the <c>SimplePrioMemoryLogger</c>.</param>
        /// <returns>The <see cref="ILoggingBuilder"/> for fluent construction.</returns>
        public static ILoggingBuilder AddSimplePrioMemoryLogger(this ILoggingBuilder builder, Action<SimplePrioMemoryLogConfig> configure)
        {
            builder.AddSimplePrioMemoryLogger();
            builder.Services.Configure(configure);
            return builder;
        }
    }
}
