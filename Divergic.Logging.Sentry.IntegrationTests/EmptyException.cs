namespace Divergic.Logging.Sentry.IntegrationTests
{
    using System;

    public class EmptyException : Exception
    {
        public Company Company { get; set; }
    }
}