namespace Divergic.Logging.Sentry.IntegrationTests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using ModelBuilder;
    using SharpRaven;
    using Xunit;

    public class SentryLoggerTests
    {
        private static readonly ISentryConfig _config = BuildConfiguration();

        [Fact]
        public async Task LogErrorSendsExceptionToSentryTest()
        {
            var client = new RavenClient(_config.Dsn)
            {
                Environment = _config.Environment,
                Release = _config.Version
            };

            var logger = new SentryLogger(typeof(SentryLoggerTests).FullName, client);
            var data = Model.Create<Person>();

            try
            {
                await RunFailure().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogErrorWithContext(ex, data);
            }
        }

        private async Task RunFailure()
        {
            var company = Model.Create<Company>();

            throw new EmptyException
            {
                Company = company
            };
        }

        private static ISentryConfig BuildConfiguration()
        {
            // Add the configuration support
            var configurationRoot = new ConfigurationBuilder()
                .AddJsonFile("integrationTests.json")
                .Build();

            return configurationRoot.Get<SentryConfig>();
        }
    }
}