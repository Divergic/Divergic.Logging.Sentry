namespace Divergic.Logging.Sentry
{
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using SharpRaven;

    /// <summary>
    ///     The <see cref="SentryLoggerProvider" />
    ///     class provides the logger for sending exceptions to Sentry.io.
    /// </summary>
    public class SentryLoggerProvider : ILoggerProvider
    {
        private readonly IRavenClient _client;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SentryLoggerProvider" /> class.
        /// </summary>
        /// <param name="client">The Sentry client.</param>
        public SentryLoggerProvider(IRavenClient client)
        {
            Ensure.Any.IsNotNull(client, nameof(client));

            _client = client;
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string categoryName)
        {
            return new SentryLogger(categoryName, _client);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // There are no resources to dispose, however this signature is required by ILoggerProvider
        }
    }
}