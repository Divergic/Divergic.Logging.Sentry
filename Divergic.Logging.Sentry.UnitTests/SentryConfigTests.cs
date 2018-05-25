namespace Divergic.Logging.Sentry.UnitTests
{
    using FluentAssertions;
    using ModelBuilder;
    using Xunit;

    public class SentryConfigTests
    {
        [Fact]
        public void CanGetAndSetPropertyValuesTest()
        {
            var sut = Model.Create<SentryConfig>();

            sut.Should().BeEquivalentTo(sut);
        }
    }
}