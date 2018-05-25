namespace Divergic.Logging.Sentry.Autofac.UnitTests
{
    using FluentAssertions;
    using global::Autofac;
    using ModelBuilder;
    using SharpRaven;
    using Xunit;

    public class SentryModuleTests
    {
        [Fact]
        public void LoadRegistersClientWithAvailableConfigurationTest()
        {
            var config = Model.Create<SentryConfig>()
                .Set(x => x.Dsn = "https://66d325a8af6145bb8998fb56db2639d9@sentry.io/228015");
            var builder = new ContainerBuilder();

            builder.RegisterInstance(config).As<ISentryConfig>();
            builder.RegisterModule<SentryModule>();

            var container = builder.Build();

            var actual = container.Resolve<IRavenClient>();

            actual.CurrentDsn.ToString().Should().Be(config.Dsn);
            actual.Environment.Should().Be(config.Environment);
            actual.Release.Should().Be(config.Version);
        }
    }
}