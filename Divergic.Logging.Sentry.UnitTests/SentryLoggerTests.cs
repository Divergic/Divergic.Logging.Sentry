namespace Divergic.Logging.Sentry.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AsyncFriendlyStackTrace;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using ModelBuilder;
    using NSubstitute;
    using SharpRaven;
    using SharpRaven.Data;
    using Xunit;

    public class SentryLoggerTests
    {
        public static IEnumerable<object[]> GetLogLevels()
        {
            var type = typeof(LogLevel);
            var values = Enum.GetValues(type);

            foreach (var value in values)
            {
                yield return new[]
                {
                    value
                };
            }
        }

        [Fact]
        public void BeginScopeReturnsInstanceTest()
        {
            var state = DateTimeOffset.UtcNow;
            var name = Guid.NewGuid().ToString();

            var client = Substitute.For<IRavenClient>();

            var sut = new SentryLogger(name, client);

            var actual = sut.BeginScope(state);

            actual.Should().NotBeNull();

            Action action = () => actual.Dispose();

            action.Should().NotThrow();
        }

        [Theory]
        [MemberData(nameof(GetLogLevels))]
        public void IsEnabledReturnsTrueForAllLevelsTest(LogLevel logLevel)
        {
            var name = Guid.NewGuid().ToString();

            var client = Substitute.For<IRavenClient>();

            var sut = new SentryLogger(name, client);

            var actual = sut.IsEnabled(logLevel);

            actual.Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void LogDoesNotIncludeCustomStringDataWithoutValueTest(string value)
        {
            var name = Guid.NewGuid().ToString();
            var eventId = new EventId(Environment.TickCount);
            var state = new AddressState
            {
                Address = Guid.NewGuid().ToString()
            };
            var exception = Model.Ignoring<ValueTypeException>(x => x.Data).Create<ValueTypeException>()
                .Set(x => x.Id = value);

            var client = Substitute.For<IRavenClient>();

            var sut = new SentryLogger(name, client);

            sut.Log(LogLevel.Critical, eventId, state, exception, (logState, ex) => ex.ToString());

            client.Received().Capture(
                Arg.Is<SentryEvent>(x => x.Exception.Data.Contains("ValueTypeException.Id") == false));
        }

        [Fact]
        public void LogDoesNotIncludeNullNestedTypeInExceptionDataTest()
        {
            var name = Guid.NewGuid().ToString();
            var eventId = new EventId(Environment.TickCount);
            var state = new AddressState
            {
                Address = Guid.NewGuid().ToString()
            };
            var exception = Model.Ignoring<WithNestedClassException>(x => x.Data).Create<WithNestedClassException>()
                .Set(x => x.State = null);

            var client = Substitute.For<IRavenClient>();

            var sut = new SentryLogger(name, client);

            sut.Log(LogLevel.Critical, eventId, state, exception, (logState, ex) => ex.ToString());

            client.Received().Capture(
                Arg.Is<SentryEvent>(x => x.Exception.Data.Contains("WithNestedClassException.State") == false));
        }

        [Fact]
        public void LogDoesNotSendEntryToSentryWhenExceptionIsNullTest()
        {
            var name = Guid.NewGuid().ToString();
            var eventId = new EventId(Environment.TickCount);
            var state = new AddressState
            {
                Address = Guid.NewGuid().ToString()
            };

            var client = Substitute.For<IRavenClient>();

            var sut = new SentryLogger(name, client);

            sut.Log(LogLevel.Critical, eventId, state, null, (logState, ex) => ex.ToString());

            client.DidNotReceive().Capture(Arg.Any<SentryEvent>());
        }

        [Fact]
        public void LogIgnoresFailureToReadPropertiesForExceptionDataTest()
        {
            var name = Guid.NewGuid().ToString();
            var eventId = new EventId(Environment.TickCount);
            var state = new AddressState
            {
                Address = Guid.NewGuid().ToString()
            };
            var exception = Model.Ignoring<ReadFailureException>(x => x.Data)
                .Ignoring<ReadFailureException>(x => x.Failure).Ignoring<ReadFailureException>(x => x.State)
                .Create<ReadFailureException>();

            var client = Substitute.For<IRavenClient>();

            var sut = new SentryLogger(name, client);

            sut.Log(LogLevel.Critical, eventId, state, exception, (logState, ex) => ex.ToString());

            client.Received().Capture(
                Arg.Is<SentryEvent>(
                    x => x.Exception.Data["ReadFailureException.Before"].As<string>() == exception.Before));
            client.Received().Capture(
                Arg.Is<SentryEvent>(x => x.Exception.Data.Contains("ReadFailureException.Failure") == false));
            client.Received().Capture(
                Arg.Is<SentryEvent>(
                    x => x.Exception.Data["ReadFailureException.Other"].As<string>() == exception.Other));
        }

        [Fact]
        public void LogSendsAggregateExceptionDetailsToSentryTest()
        {
            var name = Guid.NewGuid().ToString();
            var eventId = new EventId(Environment.TickCount);
            var state = new AddressState
            {
                Address = Guid.NewGuid().ToString()
            };
            var exception = new AggregateException(Guid.NewGuid().ToString());

            exception.WithContextData(state.Address);

            var client = Substitute.For<IRavenClient>();

            var sut = new SentryLogger(name, client);

            sut.Log(LogLevel.Critical, eventId, state, exception, (logState, ex) => ex.ToString());

            client.Received(1).Capture(Arg.Any<SentryEvent>());
            client.Received().Capture(Arg.Is<SentryEvent>(x => x.Level == ErrorLevel.Fatal));
            client.Received().Capture(Arg.Is<SentryEvent>(x => x.Exception == exception));
            client.Received().Capture(Arg.Is<SentryEvent>(x => x.Message == exception.Message));
            client.Received().Capture(Arg.Is<SentryEvent>(x => x.Tags["logger"] == name));
            client.Received().Capture(
                Arg.Is<SentryEvent>(x => x.Exception.Data["ContextData"].As<string>() == state.Address));
            client.Received().Capture(
                Arg.Is<SentryEvent>(x => x.Exception.Data["AsyncException"].As<string>() == exception.ToAsyncString()));
            client.Received().Capture(
                Arg.Is<SentryEvent>(x => x.Exception.Data["StorageException.RequestInformation"] == null));
        }

        [Fact]
        public void LogSendsAggregateExceptionWithInnerExceptionToSentryTest()
        {
            var name = Guid.NewGuid().ToString();
            var eventId = new EventId(Environment.TickCount);
            var state = new AddressState
            {
                Address = Guid.NewGuid().ToString()
            };
            var first = Model.Ignoring<ValueTypeException>(x => x.Data).Create<ValueTypeException>();
            var second = Model.Ignoring<ValueTypeException>(x => x.Data).Create<ValueTypeException>();
            var exception = new AggregateException(Guid.NewGuid().ToString(), first, second);

            exception.WithContextData(state.Address);

            var client = Substitute.For<IRavenClient>();

            var sut = new SentryLogger(name, client);

            sut.Log(LogLevel.Critical, eventId, state, exception, (logState, ex) => ex.ToString());

            client.Received().Capture(
                Arg.Is<SentryEvent>(x => x.Exception.Data["ValueTypeException.Id"].As<string>() == first.Id));
        }

        [Theory]
        [MemberData(nameof(GetLogLevels))]
        public void LogSendsExceptionToSentryTest(LogLevel logLevel)
        {
            var name = Guid.NewGuid().ToString();
            var eventId = new EventId(Environment.TickCount);
            var state = new AddressState
            {
                Address = Guid.NewGuid().ToString()
            };
            var exception = new TimeoutException(Guid.NewGuid().ToString());
            var expectedLevel = ErrorLevel.Debug;

            if (logLevel == LogLevel.Critical)
            {
                expectedLevel = ErrorLevel.Fatal;
            }
            else if (logLevel == LogLevel.Information)
            {
                expectedLevel = ErrorLevel.Info;
            }
            else if (Enum.IsDefined(typeof(ErrorLevel), logLevel.ToString()))
            {
                expectedLevel = (ErrorLevel) Enum.Parse(typeof(ErrorLevel), logLevel.ToString());
            }

            var client = Substitute.For<IRavenClient>();

            var sut = new SentryLogger(name, client);

            sut.Log(logLevel, eventId, state, exception, (logState, ex) => ex.ToString());

            client.Received(1).Capture(Arg.Any<SentryEvent>());
            client.Received().Capture(Arg.Is<SentryEvent>(x => x.Level == expectedLevel));
            client.Received().Capture(Arg.Is<SentryEvent>(x => x.Exception == exception));
            client.Received().Capture(Arg.Is<SentryEvent>(x => x.Message == exception.Message));
            client.Received().Capture(Arg.Is<SentryEvent>(x => x.Tags["logger"] == name));
            client.Received().Capture(Arg.Is<SentryEvent>(x => x.Exception.Data["ContextData"] == null));
            client.Received().Capture(Arg.Is<SentryEvent>(x => x.Exception.Data["AsyncException"] == null));
            client.Received().Capture(Arg.Is<SentryEvent>(x => x.Exception.Data["StorageException"] == null));
        }

        [Fact]
        public void LogSendsExceptionWithContextDataToSentryTest()
        {
            var name = Guid.NewGuid().ToString();
            var eventId = new EventId(Environment.TickCount);
            var state = new AddressState
            {
                Address = Guid.NewGuid().ToString()
            };
            var exception = new TimeoutException(Guid.NewGuid().ToString());

            exception.WithContextData(state.Address);

            var client = Substitute.For<IRavenClient>();

            var sut = new SentryLogger(name, client);

            sut.Log(LogLevel.Critical, eventId, state, exception, (logState, ex) => ex.ToString());

            client.Received(1).Capture(Arg.Any<SentryEvent>());
            client.Received().Capture(Arg.Is<SentryEvent>(x => x.Level == ErrorLevel.Fatal));
            client.Received().Capture(Arg.Is<SentryEvent>(x => x.Exception == exception));
            client.Received().Capture(Arg.Is<SentryEvent>(x => x.Message == exception.Message));
            client.Received().Capture(Arg.Is<SentryEvent>(x => x.Tags["logger"] == name));
            client.Received().Capture(
                Arg.Is<SentryEvent>(x => x.Exception.Data["ContextData"].As<string>().Contains(state.Address)));
        }

        [Fact]
        public void LogSendsNestedExceptionDetailsToSentryTest()
        {
            var value = Guid.NewGuid().ToString();
            var name = Guid.NewGuid().ToString();
            var eventId = new EventId(Environment.TickCount);
            var state = new AddressState
            {
                Address = Guid.NewGuid().ToString()
            };
            var innerException = Model.Ignoring<ValueTypeException>(x => x.Data).Create<ValueTypeException>()
                .Set(x => x.Id = value);
            var exception = new ArgumentNullException(Guid.NewGuid().ToString(), innerException);

            exception.WithContextData(state.Address);

            var client = Substitute.For<IRavenClient>();

            var sut = new SentryLogger(name, client);

            sut.Log(LogLevel.Critical, eventId, state, exception, (logState, ex) => ex.ToString());

            client.Received().Capture(
                Arg.Is<SentryEvent>(x => x.Exception.Data["ValueTypeException.Id"].As<string>() == value));
        }

        [Fact]
        public void LogSendsTypeReflectionLoadExceptionWithAdditionalContentTest()
        {
            var name = Guid.NewGuid().ToString();
            var eventId = new EventId(Environment.TickCount);
            var state = new AddressState
            {
                Address = Guid.NewGuid().ToString("N")
            };
            var first = Model.Ignoring<ValueTypeException>(x => x.Data).Create<ValueTypeException>();
            var second = Model.Ignoring<WithNestedClassException>(x => x.Data).Create<WithNestedClassException>();
            var innerException = new ReflectionTypeLoadException(
                new[]
                {
                    typeof(string),
                    typeof(int)
                },
                new Exception[]
                {
                    first,
                    second
                });
            var exception = new AggregateException(Guid.NewGuid().ToString(), innerException);

            exception.WithContextData(state.Address);

            var client = Substitute.For<IRavenClient>();

            var sut = new SentryLogger(name, client);

            sut.Log(LogLevel.Critical, eventId, state, exception, (logState, ex) => ex.ToString());

            client.Received().Capture(
                Arg.Is<SentryEvent>(
                    x => x.Exception.Data["ReflectionTypeLoadException.Types"].As<string>()
                        .Contains(innerException.Types[0].AssemblyQualifiedName)));
            client.Received().Capture(
                Arg.Is<SentryEvent>(
                    x => x.Exception.Data["ReflectionTypeLoadException.Types"].As<string>()
                        .Contains(innerException.Types[1].AssemblyQualifiedName)));
            client.Received().Capture(
                Arg.Is<SentryEvent>(
                    x => x.Exception.Data["ReflectionTypeLoadException.LoaderExceptions"].As<string>()
                        .Contains(innerException.LoaderExceptions[0].GetType().Name)));
            client.Received().Capture(
                Arg.Is<SentryEvent>(
                    x => x.Exception.Data["ReflectionTypeLoadException.LoaderExceptions"].As<string>()
                        .Contains(innerException.LoaderExceptions[1].GetType().Name)));
        }

        [Fact]
        public void LogSendsValueTypeExceptionPropertiesAsDataTest()
        {
            var name = Guid.NewGuid().ToString();
            var eventId = new EventId(Environment.TickCount);
            var state = new AddressState
            {
                Address = Guid.NewGuid().ToString()
            };
            var exception = Model.Ignoring<ValueTypeException>(x => x.Data).Create<ValueTypeException>();

            var client = Substitute.For<IRavenClient>();

            var sut = new SentryLogger(name, client);

            sut.Log(LogLevel.Critical, eventId, state, exception, (logState, ex) => ex.ToString());

            client.Received().Capture(
                Arg.Is<SentryEvent>(x => x.Exception.Data["ValueTypeException.Day"].As<DayOfWeek>() == exception.Day));
            client.Received().Capture(
                Arg.Is<SentryEvent>(x => x.Exception.Data["ValueTypeException.Number"].As<int>() == exception.Number));
            client.Received().Capture(
                Arg.Is<SentryEvent>(x => x.Exception.Data["ValueTypeException.Id"].As<string>() == exception.Id));
            client.Received().Capture(
                Arg.Is<SentryEvent>(
                    x => x.Exception.Data["ValueTypeException.When"].As<DateTimeOffset>() == exception.When));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ThrowsExceptionWhenCreatedWithInvalidNameTest(string name)
        {
            var client = Substitute.For<IRavenClient>();

            Action action = () => new SentryLogger(name, client);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullClientTest()
        {
            var name = Guid.NewGuid().ToString();

            Action action = () => new SentryLogger(name, null);

            action.Should().Throw<ArgumentNullException>();
        }

        private class AddressState
        {
            public string Address { get; set; }
        }

        private class ReadFailureException : Exception
        {
            public string Before { get; set; }

            public string Failure { get { throw new InvalidOperationException(); } }

            public string Other { get; set; }

            public AddressState State { get { throw new InvalidOperationException(); } }
        }

        private class ValueTypeException : Exception
        {
            public DayOfWeek Day { get; set; }

            public string Id { get; set; }

            public int Number { get; set; }

            public DateTimeOffset When { get; set; }
        }

        private class WithNestedClassException : Exception
        {
            public AddressState State { get; set; }
        }
    }
}