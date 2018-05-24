namespace Divergic.Logging.Sentry.UnitTests.Models
{
    using System;

    public class EmptyException : Exception
    {
        public Company Company { get; set; }
    }
}