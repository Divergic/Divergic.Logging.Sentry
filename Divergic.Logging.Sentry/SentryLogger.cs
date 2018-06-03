namespace Divergic.Logging.Sentry
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Internal;
    using SharpRaven;
    using SharpRaven.Data;

    /// <summary>
    ///     The <see cref="SentryLoggerProvider" />
    ///     class is used to send exception information to Sentry.io.
    /// </summary>
    public class SentryLogger : ILogger
    {
        private static readonly string _nullFormatted = new FormattedLogValues(null, null).ToString();

        private const string SentryIdKey = "Sentry_Id";
        private readonly IRavenClient _client;
        private readonly string _name;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SentryLogger" /> class.
        /// </summary>
        /// <param name="name">The name of the logger.</param>
        /// <param name="client">The Sentry.io client.</param>
        public SentryLogger(string name, IRavenClient client)
        {
            Ensure.String.IsNotNullOrEmpty(name, nameof(name));
            Ensure.Any.IsNotNull(client, nameof(client));

            _name = name;
            _client = client;
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state)
        {
            return new NullDisposable();
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            // We shouldn't assume that log levels below error are not provided exceptions
            // and we always want logged exceptions to be sent to Sentry.
            return true;
        }

        /// <inheritdoc />
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (exception == null)
            {
                return;
            }

            var recordedId = exception.Data[SentryIdKey] as string;

            if (string.IsNullOrWhiteSpace(recordedId) == false)
            {
                return;
            }

            var sentryEvent = CreateSentryEvent(logLevel, state, exception, formatter);

            _client.Logger = _name;

            var sentryId = _client.Capture(sentryEvent);

            exception.Data[SentryIdKey] = sentryId;
        }

        private static SentryEvent CreateSentryEvent<TState>(LogLevel logLevel, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            // Fix up the async/await madness
            var cleanedException = exception.Demystify();

            exception.Data["CleanedException"] = cleanedException.ToString();

            var formattedMessage = formatter(state, exception);

            // Clear the message if it looks like a null formatted message
            if (formattedMessage == _nullFormatted)
            {
                formattedMessage = null;
            }

            if (string.IsNullOrWhiteSpace(formattedMessage) == false)
            {
                exception.Data["FormattedMessage"] = formattedMessage;
            }

            StoreCustomExceptionProperties(exception, exception);

            var errorLevel = GetErrorLevel(logLevel);
            var sentryEvent = new SentryEvent(exception)
            {
                Level = errorLevel,
                Message = exception.Message,
                Tags = new ConcurrentDictionary<string, string>()
            };

            return sentryEvent;
        }

        private static ErrorLevel GetErrorLevel(LogLevel level)
        {
            if (level == LogLevel.Critical)
            {
                return ErrorLevel.Fatal;
            }

            if (level == LogLevel.Error)
            {
                return ErrorLevel.Error;
            }

            if (level == LogLevel.Warning)
            {
                return ErrorLevel.Warning;
            }

            if (level == LogLevel.Information)
            {
                return ErrorLevel.Info;
            }

            return ErrorLevel.Debug;
        }

        private static void StoreCustomExceptionProperties(Exception rootException, Exception exception)
        {
            if (exception.InnerException != null)
            {
                StoreCustomExceptionProperties(rootException, exception.InnerException);
            }

            if (exception is AggregateException aggregate)
            {
                foreach (var innerException in aggregate.InnerExceptions)
                {
                    if (innerException == exception.InnerException)
                    {
                        // Skip the exception if it is the inner exception
                        continue;
                    }

                    StoreCustomExceptionProperties(rootException, innerException);
                }

                // There are no other properties we want to track on AggregateException
                return;
            }

            var baseProperties = typeof(Exception).GetTypeInfo().GetProperties();
            var exceptionProperties = exception.GetType().GetTypeInfo().GetProperties();
            var customProperties = exceptionProperties.Except(baseProperties, new PropertyMatcher());
            var typeName = exception.GetType().GetTypeInfo().Name;

            foreach (var property in customProperties)
            {
                var keyName = typeName + "." + property.Name;

                // Check that this key has not already been assigned
                if (rootException.HasSerializedData(keyName))
                {
                    continue;
                }

                try
                {
                    var value = property.GetValue(exception);

                    if (value == null)
                    {
                        continue;
                    }

                    rootException.AddSerializedData(keyName, value);
                }
#pragma warning disable CC0004 // Catch block cannot be empty
                catch (Exception)
                {
                    // We failed to get this property, skip to the next one
                }
#pragma warning restore CC0004 // Catch block cannot be empty
            }
        }

#pragma warning disable S3881 // "IDisposable" should be implemented correctly
        private class NullDisposable : IDisposable
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
        {
            public void Dispose()
            {
                // This method is a No-op in order to provide an IDisposable instance to SentryLogger.BeginScope
            }
        }

        private class PropertyMatcher : IEqualityComparer<PropertyInfo>
        {
            public bool Equals(PropertyInfo x, PropertyInfo y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode(PropertyInfo obj)
            {
                return obj.Name.GetHashCode();
            }
        }
    }
}