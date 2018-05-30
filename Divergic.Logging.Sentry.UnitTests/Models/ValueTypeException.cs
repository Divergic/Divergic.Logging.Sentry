namespace Divergic.Logging.Sentry.UnitTests.Models
{
    using System;

    public class ValueTypeException : Exception
    {
        public DayOfWeek Day { get; set; }

        public string Id { get; set; }

        public int Number { get; set; }

        public DateTimeOffset When { get; set; }
    }
}