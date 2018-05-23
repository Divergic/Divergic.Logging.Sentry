namespace Divergic.Logging.Sentry.UnitTests
{
    using System;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using SharpRaven;
    using Xunit;

    public class LogFactoryExtensionsTests
    {
        [Fact]
        public void AddSentryAddsLoggerProviderTest()
        {
            var factory = Substitute.For<ILoggerFactory>();
            var client = Substitute.For<IRavenClient>();

            var actual = factory.AddSentry(client);

            actual.Should().Be(factory);
            factory.Received().AddProvider(Arg.Is<ILoggerProvider>(x => x is SentryLoggerProvider));
        }

        [Fact]
        public void AddSentryThrowsExceptionWithNullClientTest()
        {
            var factory = Substitute.For<ILoggerFactory>();

            Action action = () => factory.AddSentry(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddSentryThrowsExceptionWithNullFactoryTest()
        {
            var client = Substitute.For<IRavenClient>();

            ILoggerFactory factory = null;

            Action action = () => factory.AddSentry(client);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}