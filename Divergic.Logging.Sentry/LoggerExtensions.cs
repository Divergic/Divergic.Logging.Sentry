namespace Microsoft.Extensions.Logging
{
    using System;
    using Divergic.Logging.Sentry;
    using EnsureThat;
    using Microsoft.Extensions.Logging.Internal;

    /// <summary>
    /// The <see cref="LoggerExtensions"/>
    /// class provides extension methods to the <see cref="ILogger"/> interface.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Logs critical information to the specified logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="contextData">The context data to include with the exception.</param>
        public static void LogCriticalWithContext(
            this ILogger logger,
            Exception exception,
            object contextData)
        {
            LogCriticalWithContext(logger, 0, exception, contextData, null);
        }

        /// <summary>
        /// Logs critical information to the specified logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="eventId">The event id.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="contextData">The context data to include with the exception.</param>
        public static void LogCriticalWithContext(
            this ILogger logger,
            EventId eventId,
            Exception exception,
            object contextData)
        {
            LogCriticalWithContext(logger, eventId, exception, contextData, null);
        }

        /// <summary>
        /// Logs critical information to the specified logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="contextData">The context data to include with the exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The message arguments.</param>
        public static void LogCriticalWithContext(
            this ILogger logger,
            Exception exception,
            object contextData,
            string message,
            params object[] args)
        {
            LogCriticalWithContext(logger, 0, exception, contextData, message, args);
        }

        /// <summary>
        /// Logs critical information to the specified logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="eventId">The event id.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="contextData">The context data to include with the exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The message arguments.</param>
        public static void LogCriticalWithContext(
            this ILogger logger,
            EventId eventId,
            Exception exception,
            object contextData,
            string message,
            params object[] args)
        {
            Ensure.Any.IsNotNull(logger, nameof(logger));

            if (contextData != null)
            {
                exception.AddContextData(contextData);
            }

            logger.Log<object>(
                LogLevel.Critical,
                eventId,
                new FormattedLogValues(message, args),
                exception,
                MessageFormatter);
        }

        /// <summary>
        /// Logs error information to the specified logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="contextData">The context data to include with the exception.</param>
        public static void LogErrorWithContext(
            this ILogger logger,
            Exception exception,
            object contextData)
        {
            LogErrorWithContext(logger, 0, exception, contextData, null);
        }

        /// <summary>
        /// Logs error information to the specified logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="eventId">The event id.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="contextData">The context data to include with the exception.</param>
        public static void LogErrorWithContext(
            this ILogger logger,
            EventId eventId,
            Exception exception,
            object contextData)
        {
            LogErrorWithContext(logger, eventId, exception, contextData, null);
        }

        /// <summary>
        /// Logs error information to the specified logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="contextData">The context data to include with the exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The message arguments.</param>
        public static void LogErrorWithContext(
            this ILogger logger,
            Exception exception,
            object contextData,
            string message,
            params object[] args)
        {
            LogErrorWithContext(logger, 0, exception, contextData, message, args);
        }

        /// <summary>
        /// Logs error information to the specified logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="eventId">The event id.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="contextData">The context data to include with the exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The message arguments.</param>
        public static void LogErrorWithContext(
            this ILogger logger,
            EventId eventId,
            Exception exception,
            object contextData,
            string message,
            params object[] args)
        {
            Ensure.Any.IsNotNull(logger, nameof(logger));

            if (contextData != null)
            {
                exception.AddContextData(contextData);
            }

            logger.Log<object>(
                LogLevel.Error,
                eventId,
                new FormattedLogValues(message, args),
                exception,
                MessageFormatter);
        }

        private static string MessageFormatter(object state, Exception error)
        {
            return state?.ToString();
        }
    }
}