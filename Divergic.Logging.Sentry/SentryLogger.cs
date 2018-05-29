namespace Divergic.Logging.Sentry
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AsyncFriendlyStackTrace;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using SharpRaven;
    using SharpRaven.Data;

    /// <summary>
    ///     The <see cref="SentryLoggerProvider" />
    ///     class is used to send exception information to Sentry.io.
    /// </summary>
    public class SentryLogger : ILogger
    {
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

            var aggregate = exception as AggregateException;

            if (aggregate != null)
            {
                exception.Data["AsyncException"] = exception.ToAsyncString();
            }

            StoreCustomExceptionProperties(exception, exception);

            var errorLevel = GetErrorLevel(logLevel);
            var sentryEvent = new SentryEvent(exception)
            {
                Level = errorLevel,
                Message = exception.Message,
                Tags = new ConcurrentDictionary<string, string>()
            };

            _client.Logger = _name;

            var sentryId = _client.Capture(sentryEvent);

            exception.Data[SentryIdKey] = sentryId;
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

        private static object GetPropertyValue(Exception exception, PropertyInfo property)
        {
            if (property.PropertyType.IsValueType)
            {
                // Push this value as is
                var value = property.GetValue(exception);

                return value;
            }

            if (property.PropertyType == typeof(string))
            {
                // Push this value as is
                var value = property.GetValue(exception) as string;

                if (string.IsNullOrWhiteSpace(value))
                {
                    return null;
                }

                return value;
            }

            try
            {
                // Attempt to serialize this value
                var value = property.GetValue(exception);

                if (value == null)
                {
                    return null;
                }

                var serialized = JsonConvert.SerializeObject(value, ContextData.SerializerSettings);

                if (serialized == "{}")
                {
                    return null;
                }

                return serialized;
            }
#pragma warning disable CC0004 // Catch block cannot be empty
            catch (Exception)
            {
                // We failed to serialize this value so ignore it
                return null;
            }
#pragma warning restore CC0004 // Catch block cannot be empty
        }

        private static void StoreCustomExceptionProperties(Exception rootException, Exception exception)
        {
            if (exception.InnerException != null)
            {
                StoreCustomExceptionProperties(rootException, exception.InnerException);
            }

            var aggregate = exception as AggregateException;

            if (aggregate != null)
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
                if (rootException.Data.Contains(keyName))
                {
                    continue;
                }

                try
                {
                    var value = GetPropertyValue(exception, property);

                    if (value == null)
                    {
                        continue;
                    }

                    rootException.Data[keyName] = value;
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