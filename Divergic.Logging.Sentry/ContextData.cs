namespace System
{
    using System.Diagnostics;
    using System.Reflection;
    using EnsureThat;
    using Newtonsoft.Json;

    /// <summary>
    /// The <see cref="ContextData"/>
    /// class provides extension methods for the <see cref="Exception"/> class.
    /// </summary>
    public static class ContextData
    {
        /// <summary>
        /// The key value used to store context data in <see cref="Exception.Data"/>.
        /// </summary>
        public const string ContextDataKey = "ContextData";

        /// <summary>
        /// Adds context data to the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="contextData">The context data.</param>
        /// <returns>The exception with context data appended.</returns>
        public static Exception AddContextData(this Exception exception, object contextData)
        {
            Ensure.Any.IsNotNull(exception, nameof(exception));
            Ensure.Any.IsNotNull(contextData, nameof(contextData));

            if (HasContextData(exception))
            {
                return exception;
            }

            var data = ConvertToString(contextData);

            exception.Data.Add(ContextDataKey, data);

            return exception;
        }

        private static JsonSerializerSettings BuildSerializerSettings()
        {
            var settings = new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.None
            };

            return settings;
        }

        private static string ConvertToString(object contextData)
        {
            Debug.Assert(contextData != null, "No context data provided");

            if (contextData.GetType().GetTypeInfo().IsValueType)
            {
                return contextData.ToString();
            }

            var dataAsString = contextData as string;

            if (dataAsString != null)
            {
                return dataAsString;
            }

            try
            {
                var serializedData = JsonConvert.SerializeObject(contextData, SerializerSettings);

                return serializedData;
            }
            catch (Exception)
            {
                return contextData.ToString();
            }
        }

        private static bool HasContextData(Exception exception)
        {
            // The exception comes from DomainExceptionEventArgs which ensures that the exception exists
            Debug.Assert(exception != null, "No exception was provided");

            var contextData = exception.Data[ContextDataKey] as string;

            if (string.IsNullOrWhiteSpace(contextData))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the serializer settings used to append context data to exceptions.
        /// </summary>
        public static JsonSerializerSettings SerializerSettings { get; } = BuildSerializerSettings();
    }
}