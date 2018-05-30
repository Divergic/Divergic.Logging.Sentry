namespace Divergic.Logging.Sentry.IntegrationTests
{
    using System;
    using NodaTime;

    public class CustomPropertyException : Exception
    {
        public Company Company { get; set; }

        public Instant Point { get; set; }

        public int Value { get; set; }
    }
}