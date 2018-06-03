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
        private static readonly string _nullFormatted = new FormattedLogValues(null, null).ToString();

        /// <summary>
        /// Logs critical information to the specified logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="contextData">The context data to include with the exception.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="exception"/> is <c>null</c>.</exception>
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
        /// <exception cref="ArgumentNullException">The <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="exception"/> is <c>null</c>.</exception>
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
        /// <exception cref="ArgumentNullException">The <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="exception"/> is <c>null</c>.</exception>
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
        /// <exception cref="ArgumentNullException">The <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="exception"/> is <c>null</c>.</exception>
        public static void LogCriticalWithContext(
            this ILogger logger,
            EventId eventId,
            Exception exception,
            object contextData,
            string message,
            params object[] args)
        {
            WriteMessage(logger, LogLevel.Critical, eventId, exception, contextData, message, args);
        }

        /// <summary>
        /// Logs error information to the specified logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="contextData">The context data to include with the exception.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="exception"/> is <c>null</c>.</exception>
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
        /// <exception cref="ArgumentNullException">The <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="exception"/> is <c>null</c>.</exception>
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
        /// <exception cref="ArgumentNullException">The <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="exception"/> is <c>null</c>.</exception>
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
        /// <exception cref="ArgumentNullException">The <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="exception"/> is <c>null</c>.</exception>
        public static void LogErrorWithContext(
            this ILogger logger,
            EventId eventId,
            Exception exception,
            object contextData,
            string message,
            params object[] args)
        {
            Ensure.Any.IsNotNull(logger, nameof(logger));

            WriteMessage(logger, LogLevel.Error, eventId, exception, contextData, message, args);
        }

        private static string MessageFormatter(object state, Exception error)
        {
            return state?.ToString();
        }

        private static void WriteMessage(ILogger logger, LogLevel logLevel, EventId eventId, Exception exception,
            object contextData, string message, object[] args)
        {
            Ensure.Any.IsNotNull(logger, nameof(logger));
            Ensure.Any.IsNotNull(exception, nameof(exception));

            if (contextData != null)
            {
                exception.AddContextData(contextData);
            }

            object formattedMessage = null;

            if (string.IsNullOrWhiteSpace(message) == false)
            {
                formattedMessage = new FormattedLogValues(message, args);
            }

            logger.Log(
                logLevel,
                eventId,
                formattedMessage,
                exception,
                MessageFormatter);
        }
    }
}