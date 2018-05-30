namespace Divergic.Logging.Sentry.IntegrationTests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using ModelBuilder;
    using NodaTime;
    using NodaTime.Serialization.JsonNet;
    using SharpRaven;
    using Xunit;

    public class SentryLoggerTests
    {
        private static readonly IRavenClient _client;
        private static readonly ISentryConfig _config;

        static SentryLoggerTests()
        {
            _config = BuildConfiguration();
            _client = new RavenClient(_config.Dsn)
            {
                Environment = _config.Environment,
                Release = _config.Version
            };

            ExceptionData.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        }

        [Fact]
        public async Task LogErrorSendsExceptionToSentryTest()
        {
            var logger = new SentryLogger(typeof(SentryLoggerTests).FullName, _client);
            var data = Model.Ignoring<Person>(x => x.CreatedAt).Create<Person>().Set(x => x.CreatedAt = SystemClock.Instance.GetCurrentInstant());

            try
            {
                await RunFailure().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogErrorWithContext(ex, data);
            }
        }

        private static ISentryConfig BuildConfiguration()
        {
            // Add the configuration support
            var configurationRoot = new ConfigurationBuilder()
                .AddJsonFile("integrationTests.json")
                .Build();

            return configurationRoot.Get<SentryConfig>();
        }

        private async Task RunFailure()
        {
            var company = Model
                .Ignoring<Person>(x => x.CreatedAt)
                .Create<Company>().Set(x =>
                {
                    x.Owner.CreatedAt = SystemClock.Instance.GetCurrentInstant();
                });

            throw new CustomPropertyException
            {
                Company = company,
                Value = Environment.TickCount,
                Point = SystemClock.Instance.GetCurrentInstant()
            };
        }
    }
}