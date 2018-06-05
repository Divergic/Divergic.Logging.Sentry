namespace Divergic.Logging.Sentry.UnitTests.Models
{
    using System;

    public class ReadFailureException : Exception
    {
        public string Before { get; set; }

        public string Failure { get { throw new InvalidOperationException(); } }

        public string Other { get; set; }

        public AddressState State { get { throw new InvalidOperationException(); } }
    }
}