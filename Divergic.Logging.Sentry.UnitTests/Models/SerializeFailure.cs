namespace Divergic.Logging.Sentry.UnitTests.Models
{
    using System;

    public class SerializeFailure
    {
        private string _name;

        public string Name { get => throw new InvalidOperationException(); set => _name = value; }
    }
}