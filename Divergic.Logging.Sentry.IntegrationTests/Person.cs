namespace Divergic.Logging.Sentry.IntegrationTests
{
    using NodaTime;

    public class Person
    {
        public Instant CreatedAt { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}