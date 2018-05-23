namespace Divergic.Logging.Sentry.Autofac
{
    using global::Autofac;
    using SharpRaven;

    /// <summary>
    ///     The <see cref="SentryModule" />
    ///     class is used to configure a <see cref="IRavenClient" /> for sending errors to Sentry.io.
    /// </summary>
    public class SentryModule : Module
    {
        /// <inheritdoc />
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(x =>
            {
                var config = x.Resolve<ISentryConfig>();

                var client = new RavenClient(config.Dsn)
                {
                    Environment = config.Environment,
                    Release = config.Version
                };

                return client;
            });
        }
    }
}