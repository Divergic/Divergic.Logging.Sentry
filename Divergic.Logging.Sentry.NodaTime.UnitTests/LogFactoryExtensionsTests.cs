namespace Divergic.Logging.Sentry.NodaTime.UnitTests
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using SharpRaven;
    using Xunit;
    using Xunit.Abstractions;

    public class LogFactoryExtensionsTests
    {
        private readonly ITestOutputHelper _output;

        public LogFactoryExtensionsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void AddSentryWithNodaTimeAddsLoggerProviderTest()
        {
            var factory = Substitute.For<ILoggerFactory>();
            var client = Substitute.For<IRavenClient>();

            var actual = factory.AddSentryWithNodaTime(client);

            actual.Should().Be(factory);
            factory.Received().AddProvider(Arg.Is<ILoggerProvider>(x => x is SentryLoggerProvider));
        }

        [Fact]
        public void AddSentryWithNodaTimeConfiguresContextDataSerializerWithNodaTimeTest()
        {
            var factory = Substitute.For<ILoggerFactory>();
            var client = Substitute.For<IRavenClient>();

            factory.AddSentryWithNodaTime(client);

            ContextData.SerializerSettings.Converters.Any(x => x.GetType().FullName.StartsWith("NodaTime.Serialization.JsonNet.")).Should().BeTrue();
        }

        [Fact]
        public void AddSentryWithNodaTimeThrowsExceptionWithNullClientTest()
        {
            var factory = Substitute.For<ILoggerFactory>();

            Action action = () => factory.AddSentryWithNodaTime(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddSentryWithNodaTimeThrowsExceptionWithNullFactoryTest()
        {
            var client = Substitute.For<IRavenClient>();

            ILoggerFactory factory = null;

            Action action = () => factory.AddSentryWithNodaTime(client);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}