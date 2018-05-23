namespace Divergic.Logging.Sentry.UnitTests
{
    using System;
    using FluentAssertions;
    using NSubstitute;
    using SharpRaven;
    using Xunit;

    public class SentryLoggerProviderTests
    {
        [Fact]
        public void CanDisposeTest()
        {
            var client = Substitute.For<IRavenClient>();

            using (var sut = new SentryLoggerProvider(client))
            {
                Action action = () => sut.Dispose();

                action.Should().NotThrow();
            }
        }

        [Fact]
        public void CreateLoggerReturnsLoggerTest()
        {
            var name = Guid.NewGuid().ToString();

            var client = Substitute.For<IRavenClient>();

            using (var sut = new SentryLoggerProvider(client))
            {
                var actual = sut.CreateLogger(name);

                actual.Should().NotBeNull();
            }
        }

        [Fact]
        public void ThrowsExceptionWithNullClientTest()
        {
            Action action = () =>
            {
                using (new SentryLoggerProvider(null))
                {
                    // No-op
                }
            };

            action.Should().Throw<ArgumentNullException>();
        }
    }
}