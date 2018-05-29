namespace Divergic.Logging.Sentry.IntegrationTests
{
    using System;

    public class CustomPropertyException : Exception
    {
        public Company Company { get; set; }
    }
}