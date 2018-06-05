namespace Divergic.Logging.Sentry.UnitTests
{
    using System;
    using System.Collections.Generic;
    using Divergic.Logging.Sentry.UnitTests.Models;
    using FluentAssertions;
    using ModelBuilder;
    using Newtonsoft.Json;
    using NodaTime;
    using NodaTime.Serialization.JsonNet;
    using Xunit;
    using Xunit.Abstractions;

    public class ExceptionDataTests
    {
        private readonly ITestOutputHelper _output;

        static ExceptionDataTests()
        {
            ExceptionData.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        }

        public ExceptionDataTests(ITestOutputHelper output)
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
        public void AddContextDataAppendsSerializedValueToExceptionTest()
        {
            var value = Model.Create<Company>();
            var expected = JsonConvert.SerializeObject(value, ExceptionData.SerializerSettings);

            var sut = new TimeoutException();

            sut.AddContextData(value);

            var actual = sut.Data["ContextData"].As<string>();

            _output.WriteLine("Stored context data is: {0}", actual);

            actual.Should().Be(expected);
        }

        [Fact]
        public void AddSerializedDataAppendsDataToStringWhenSerializationFailsTest()
        {
            var key = Guid.NewGuid().ToString("N");
            var value = new SerializeFailure();

            var sut = new TimeoutException();

            sut.AddSerializedData(key, value);

            var actual = sut.Data[key].As<string>();

            _output.WriteLine("Stored data is: {0}", actual);

            actual.Should().Be(typeof(SerializeFailure).FullName);
        }

        [Fact]
        public void AddSerializedDataAppendsSerializedNonSystemValueTypeTest()
        {
            var key = Guid.NewGuid().ToString("N");
            var value = SystemClock.Instance.GetCurrentInstant();
            var expected = JsonConvert.SerializeObject(value, ExceptionData.SerializerSettings);

            var sut = new TimeoutException();

            sut.AddSerializedData(key, value);

            var actual = sut.Data[key];

            _output.WriteLine("Stored data is: {0}", actual);

            actual.Should().Be(expected);
        }

        [Fact]
        public void AddSerializedDataAppendsSerializedValueToExceptionTest()
        {
            var key = Guid.NewGuid().ToString("N");
            var value = Model.Create<Company>();
            var expected = JsonConvert.SerializeObject(value, ExceptionData.SerializerSettings);

            var sut = new TimeoutException();

            sut.AddSerializedData(key, value);

            var actual = sut.Data[key].As<string>();

            _output.WriteLine("Stored data is: {0}", actual);

            actual.Should().Be(expected);
        }

        [Theory]
        [MemberData(nameof(ValueTypeValues))]
        public void AddSerializedDataAppendsValueTypeValuesTest(object value)
        {
            var key = Guid.NewGuid().ToString("N");

            var sut = new TimeoutException();

            sut.AddSerializedData(key, value);

            var actual = sut.Data[key];

            _output.WriteLine("Stored data is: {0}", actual);

            actual.Should().Be(value);
        }

        [Fact]
        public void AddSerializedDataDoesNotAppendValueWhenDataAlreadyStoredTest()
        {
            var key = Guid.NewGuid().ToString("N");
            var value = Guid.NewGuid().ToString();
            var nextValue = Guid.NewGuid().ToString();

            var sut = new TimeoutException();

            sut.AddSerializedData(key, value);
            sut.AddSerializedData(key, nextValue);

            var actual = sut.Data[key];

            _output.WriteLine("Stored data is: {0}", actual);

            actual.Should().Be(value);
        }

        [Fact]
        public void AddSerializedDataDoesNotIncludeNullNestedTypeInExceptionDataTest()
        {
            var key = Guid.NewGuid().ToString();
            var value = Model.Ignoring<Company>(x => x.Owner).Create<Company>();

            var sut = new TimeoutException();

            sut.AddSerializedData(key, value);

            var expected = sut.Data[key] as string;

            expected.Should().NotContain(typeof(SerializeFailure).FullName);
        }

        [Fact]
        public void AddSerializedDataIgnoresFailureToReadPropertiesForExceptionDataTest()
        {
            var key = Guid.NewGuid().ToString();
            var value = new SerializeFailure
                {Name = Guid.NewGuid().ToString()};

            var sut = new TimeoutException();

            sut.AddSerializedData(key, value);

            var expected = sut.Data[key] as string;

            expected.Should().Be(typeof(SerializeFailure).FullName);
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("  ", false)]
        [InlineData("Stuff", true)]
        public void AddSerializedDataOnlyAppendsStringsWithContentsTest(string value, bool expected)
        {
            var key = Guid.NewGuid().ToString("N");

            var sut = new TimeoutException();

            sut.AddSerializedData(key, value);

            var actual = sut.Data[key];

            _output.WriteLine("Stored data is: {0}", actual);

            if (expected)
            {
                actual.Should().Be(value);
            }
            else
            {
                sut.Data.Contains(key).Should().BeFalse();
                actual.Should().BeNull();
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void AddSerializedDataThrowsExceptionWithInvalidKeyTest(string key)
        {
            var value = Guid.NewGuid().ToString();

            var sut = new TimeoutException();

            Action action = () => sut.AddSerializedData(key, value);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void AddSerializedDataThrowsExceptionWithNullDataTest()
        {
            var key = Guid.NewGuid().ToString("N");

            var sut = new TimeoutException();

            Action action = () => sut.AddSerializedData(key, null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddSerializedDataThrowsExceptionWithNullExceptionTest()
        {
            var key = Guid.NewGuid().ToString("N");
            var value = Guid.NewGuid().ToString();
            var sut = (Exception) null;

            Action action = () => sut.AddSerializedData(key, value);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DefaultSerializerSettingsReturnsNewDefaultValueTest()
        {
            ExceptionData.DefaultSerializerSettings.MaxDepth = Environment.TickCount;

            ExceptionData.DefaultSerializerSettings.MaxDepth.Should().BeNull();
        }

        [Fact]
        public void HasSerializedDataReturnsWhetherExceptionContainsDataTest()
        {
            var key = Guid.NewGuid().ToString("N");
            var value = Guid.NewGuid().ToString();
            var keyNotFound = Guid.NewGuid().ToString("N");

            var sut = new TimeoutException();

            sut.Data.Add(key, value);

            sut.HasSerializedData(key).Should().BeTrue();
            sut.HasSerializedData(keyNotFound).Should().BeFalse();
        }

        [Fact]
        public void SerializerSettingsCanAssignDefaultSerializerSettingsTest()
        {
            ExceptionData.SerializerSettings.MaxDepth = Environment.TickCount;

            ExceptionData.SerializerSettings = ExceptionData.DefaultSerializerSettings;

            ExceptionData.SerializerSettings.MaxDepth.Should().BeNull();
        }
    }
}