namespace Divergic.Logging.Sentry.UnitTests
{
    using System;
    using System.Collections.Generic;
    using Divergic.Logging.Sentry.UnitTests.Models;
    using FluentAssertions;
    using ModelBuilder;
    using Xunit;
    using Xunit.Abstractions;

    public class ContextDataTests
    {
        private readonly ITestOutputHelper _output;

        public ContextDataTests(ITestOutputHelper output)
        {
            _output = output;
        }

        public static IEnumerable<object[]> ValueTypeValues()
        {
            yield return new object[] {Environment.TickCount};
            yield return new object[] {true};
            yield return new object[] {false};
            yield return new object[] {DateTimeOffset.UtcNow};
            yield return new object[] {Guid.NewGuid()};
        }

        [Fact]
        public void WithContextDataAppendsDataToStringWhenSerializationFailsTest()
        {
            var value = new SerializeFailure();

            var sut = new TimeoutException();

            sut.WithContextData(value);

            var actual = sut.Data[ContextData.ContextDataKey].As<string>();

            _output.WriteLine("Stored context data is: {0}", actual);

            actual.Should().Be(typeof(SerializeFailure).FullName);
        }

        [Fact]
        public void WithContextDataAppendsReferenceValueTest()
        {
            var value = Model.Create<Company>();

            var sut = new TimeoutException();

            sut.WithContextData(value);

            var actual = sut.Data[ContextData.ContextDataKey].As<string>();

            _output.WriteLine("Stored context data is: {0}", actual);

            actual.Should().Contain(value.Address);
            actual.Should().Contain(value.Name);
            actual.Should().Contain(value.Owner.FirstName);

            foreach (var person in value.Staff)
            {
                actual.Should().Contain(person.FirstName);
            }
        }

        [Fact]
        public void WithContextDataAppendsStringValueTest()
        {
            var value = Guid.NewGuid().ToString();

            var sut = new TimeoutException();

            sut.WithContextData(value);

            var actual = sut.Data[ContextData.ContextDataKey];

            _output.WriteLine("Stored context data is: {0}", actual);

            actual.Should().Be(value);
        }

        [Theory]
        [MemberData(nameof(ValueTypeValues))]
        public void WithContextDataAppendsValueTypeValuesTest(object value)
        {
            var sut = new TimeoutException();

            sut.WithContextData(value);

            var actual = sut.Data[ContextData.ContextDataKey];

            _output.WriteLine("Stored context data is: {0}", actual);

            actual.Should().Be(value.ToString());
        }

        [Fact]
        public void WithContextDataDoesNotAppendValueWhenDataAlreadyStoredTest()
        {
            var value = Guid.NewGuid().ToString();
            var nextValue = Guid.NewGuid().ToString();

            var sut = new TimeoutException();

            sut.WithContextData(value);
            sut.WithContextData(nextValue);

            var actual = sut.Data[ContextData.ContextDataKey];

            _output.WriteLine("Stored context data is: {0}", actual);

            actual.Should().Be(value);
        }

        [Fact]
        public void WithContextDataThrowsExceptionWithNullContextDataTest()
        {
            var sut = new TimeoutException();

            Action action = () => sut.WithContextData(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WithContextDataThrowsExceptionWithNullExceptionTest()
        {
            var sut = (Exception) null;

            Action action = () => sut.WithContextData(Guid.NewGuid().ToString());

            action.Should().Throw<ArgumentNullException>();
        }
    }
}