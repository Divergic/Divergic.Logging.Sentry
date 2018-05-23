namespace Divergic.Logging.Sentry
{
    /// <summary>
    ///     The <see cref="ISentryConfig" />
    ///     interface defines the configuration options for sending errors to Sentry.io.
    /// </summary>
    public interface ISentryConfig
    {
        /// <summary>
        ///     Gets the Dsn.
        /// </summary>
        string Dsn { get; }

        /// <summary>
        ///     Gets the environment.
        /// </summary>
        string Environment { get; }

        /// <summary>
        ///     Gets the application version.
        /// </summary>
        string Version { get; }
    }
}