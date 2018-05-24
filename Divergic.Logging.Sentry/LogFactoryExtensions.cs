namespace Microsoft.Extensions.Logging
{
    using Divergic.Logging.Sentry;
    using EnsureThat;
    using SharpRaven;

    /// <summary>
    ///     The <see cref="LogFactoryExtensions" />
    ///     class provides extension methods for managing a <see cref="ILoggerFactory" />.
    /// </summary>
    public static class LogFactoryExtensions
    {
        /// <summary>
        /// Adds the Sentry logger provider to the specified factory
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="client">The Sentry client.</param>
        /// <returns>The logger factory</returns>
        public static ILoggerFactory AddSentry(this ILoggerFactory factory, IRavenClient client)
        {
            Ensure.Any.IsNotNull(factory, nameof(factory));
            Ensure.Any.IsNotNull(client, nameof(client));

            var provider = new SentryLoggerProvider(client);

            factory.AddProvider(provider);

            return factory;
        }
    }
}