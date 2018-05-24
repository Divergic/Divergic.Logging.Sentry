namespace Divergic.Logging.Sentry.UnitTests.Models
{
    using System.Collections.Generic;

    public class Company
    {
        public string Address { get; set; }

        public string Name { get; set; }

        public Person Owner { get; set; }

        public IEnumerable<Person> Staff { get; set; }
    }
}