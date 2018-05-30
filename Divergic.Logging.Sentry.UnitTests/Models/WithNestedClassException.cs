namespace Divergic.Logging.Sentry.UnitTests.Models
{
    using System;

    public class WithNestedClassException : Exception
    {
        public AddressState State { get; set; }
    }
}