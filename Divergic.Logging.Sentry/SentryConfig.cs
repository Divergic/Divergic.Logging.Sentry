namespace Divergic.Logging.Sentry
{
    /// <summary>
    ///     The <see cref="SentryConfig" />
    ///     class is used to define the configuration values for communicating with Sentry.io.
    /// </summary>
    public class SentryConfig : ISentryConfig
    {
        /// <summary>
        ///     Gets or sets the Dsn.
        /// </summary>
        public string Dsn { get; set; }

        /// <summary>
        ///     Gets or sets the environment.
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        ///     Gets or sets the application version.
        /// </summary>
        public string Version { get; set; }
    }
}