namespace Divergic.Logging.Sentry.UnitTests
{
    using System;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using Xunit;

    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogCriticalWithContextIgnoresNullDataTest()
        {
            var eventId = new EventId(Environment.TickCount);
            var exception = new TimeoutException();
            const string Message = "{0}-{1}";
            var args = new object[]
            {
                123,
                true
            };

            var log = Substitute.For<ILogger>();

            log.LogCriticalWithContext(eventId, exception, null, Message, args);

            log.Received().Log(
                LogLevel.Critical,
                eventId,
                Arg.Is<object>(x => x.ToString() == "123-True"),
                Arg.Is<Exception>(x => x == exception && x.Data["ContextData"] == null),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public void LogCriticalWithContextLogsExceptionWithMessageTest()
        {
            var eventId = new EventId(Environment.TickCount);
            var exception = new TimeoutException();
            var contextData = Guid.NewGuid().ToString();
            const string Message = "{0}-{1}";
            var args = new object[]
            {
                123,
                true
            };

            var log = Substitute.For<ILogger>();

            log.LogCriticalWithContext(eventId, exception, contextData, Message, args);

            log.Received().Log(
                LogLevel.Critical,
                eventId,
                Arg.Is<object>(x => x.ToString() == "123-True"),
                Arg.Is<Exception>(x => x == exception && x.Data["ContextData"].ToString().Contains(contextData)),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public void LogCriticalWithContextLogsExceptionWithoutMessageTest()
        {
            var eventId = new EventId(Environment.TickCount);
            var exception = new TimeoutException();
            var contextData = Guid.NewGuid().ToString();

            var log = Substitute.For<ILogger>();

            log.LogCriticalWithContext(eventId, exception, contextData);

            log.Received().Log(
                LogLevel.Critical,
                eventId,
                Arg.Any<object>(),
                Arg.Is<Exception>(x => x == exception && x.Data["ContextData"].ToString().Contains(contextData)),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public void LogCriticalWithContextThrowsExceptionWithNullLogTest()
        {
            var eventId = new EventId(Environment.TickCount);
            var exception = new TimeoutException();
            var contextData = Guid.NewGuid().ToString();
            const string Message = "{0}-{1}";
            var args = new object[]
            {
                123,
                true
            };

            var log = (ILogger)null;

            Action action = () => log.LogCriticalWithContext(eventId, exception, contextData, Message, args);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void LogErrorWithContextIgnoresNullDataTest()
        {
            var eventId = new EventId(Environment.TickCount);
            var exception = new TimeoutException();
            const string Message = "{0}-{1}";
            var args = new object[]
            {
                123,
                true
            };

            var log = Substitute.For<ILogger>();

            log.LogErrorWithContext(eventId, exception, null, Message, args);

            log.Received().Log(
                LogLevel.Error,
                eventId,
                Arg.Is<object>(x => x.ToString() == "123-True"),
                Arg.Is<Exception>(x => x == exception && x.Data.Contains("ContextData") == false),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public void LogErrorWithContextLogsExceptionWithMessageTest()
        {
            var eventId = new EventId(Environment.TickCount);
            var exception = new TimeoutException();
            var contextData = Guid.NewGuid().ToString();
            const string Message = "{0}-{1}";
            var args = new object[]
            {
                123,
                true
            };

            var log = Substitute.For<ILogger>();

            log.LogErrorWithContext(eventId, exception, contextData, Message, args);

            log.Received().Log(
                LogLevel.Error,
                eventId,
                Arg.Is<object>(x => x.ToString() == "123-True"),
                Arg.Is<Exception>(x => x == exception && x.Data["ContextData"].ToString().Contains(contextData)),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public void LogErrorWithContextLogsExceptionWithoutMessageTest()
        {
            var eventId = new EventId(Environment.TickCount);
            var exception = new TimeoutException();
            var contextData = Guid.NewGuid().ToString();

            var log = Substitute.For<ILogger>();

            log.LogErrorWithContext(eventId, exception, contextData);

            log.Received().Log(
                LogLevel.Error,
                eventId,
                Arg.Any<object>(),
                Arg.Is<Exception>(x => x == exception && x.Data["ContextData"].ToString().Contains(contextData)),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public void LogErrorWithContextThrowsExceptionWithNullLogTest()
        {
            var eventId = new EventId(Environment.TickCount);
            var exception = new TimeoutException();
            var contextData = Guid.NewGuid().ToString();
            const string Message = "{0}-{1}";
            var args = new object[]
            {
                123,
                true
            };

            var log = (ILogger)null;

            Action action = () => log.LogErrorWithContext(eventId, exception, contextData, Message, args);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}