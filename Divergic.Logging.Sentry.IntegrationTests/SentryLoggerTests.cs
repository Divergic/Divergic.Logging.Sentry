namespace Divergic.Logging.Sentry.IntegrationTests
{
    using System;
    using System.Threading.Tasks;
    using Divergic.Logging.Xunit;
    using global::Xunit;
    using global::Xunit.Abstractions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using ModelBuilder;
    using NodaTime;
    using SharpRaven;

    public class SentryLoggerTests
    {
        private readonly ILoggerFactory _factory;

        public SentryLoggerTests(ITestOutputHelper output)
        {
            var config = BuildConfiguration();

            var client = new RavenClient(config.Dsn)
            {
                Environment = config.Environment,
                Release = config.Version
            };

            _factory = LogFactory.Create(output).AddSentryWithNodaTime(client);
        }

        [Fact]
        public async Task LogComplexAsyncErrorSendsExceptionToSentryTest()
        {
            var logger = _factory.CreateLogger(nameof(SentryLoggerTests));
            var data = Model.Ignoring<Person>(x => x.CreatedAt).Create<Person>()
                .Set(x => x.CreatedAt = SystemClock.Instance.GetCurrentInstant());

            try
            {
                await FailureGenerator.Execute().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogErrorWithContext(ex, data);
            }
        }

        [Fact]
        public async Task LogErrorSendsExceptionToSentryTest()
        {
            var logger = _factory.CreateLogger(nameof(SentryLoggerTests));
            var data = Model.Ignoring<Person>(x => x.CreatedAt).Create<Person>()
                .Set(x => x.CreatedAt = SystemClock.Instance.GetCurrentInstant());

            try
            {
                await RunFailure().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogErrorWithContext(ex, data);
            }
        }

        [Fact]
        public void LogSimpleErrorSendsExceptionToSentryTest()
        {
            var logger = _factory.CreateLogger(nameof(SentryLoggerTests));
            var data = Model.Ignoring<Person>(x => x.CreatedAt).Create<Person>()
                .Set(x => x.CreatedAt = SystemClock.Instance.GetCurrentInstant());

            try
            {
                SimpleFailure();
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

        private Task RunFailure()
        {
            var company = Model
                .Ignoring<Person>(x => x.CreatedAt)
                .Create<Company>().Set(x => { x.Owner.CreatedAt = SystemClock.Instance.GetCurrentInstant(); });

            throw new CustomPropertyException
            {
                Company = company,
                Value = Environment.TickCount,
                Point = SystemClock.Instance.GetCurrentInstant()
            };
        }

        private void SimpleFailure()
        {
            throw new TimeoutException();
        }
    }
}