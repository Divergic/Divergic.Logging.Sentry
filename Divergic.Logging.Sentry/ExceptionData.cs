namespace Divergic.Logging.Sentry
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using EnsureThat;
    using Newtonsoft.Json;

    /// <summary>
    /// The <see cref="ExceptionData"/>
    /// class provides extension methods for the <see cref="Exception"/> class.
    /// </summary>
    public static class ExceptionData
    {
        private const string ContextDataKey = "ContextData";

        /// <summary>
        /// Adds context data to the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="contextData">The context data.</param>
        /// <returns>The exception with context data appended.</returns>
        public static Exception AddContextData(this Exception exception, object contextData)
        {
            return AddSerializedData(exception, ContextDataKey, contextData);
        }

        /// <summary>
        /// Adds the specified data to the exception as a JSON serialized value.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="key">The key used to identify the data.</param>
        /// <param name="data">The data to store.</param>
        /// <returns>The exception with context data appended.</returns>
        public static Exception AddSerializedData(this Exception exception, string key, object data)
        {
            Ensure.Any.IsNotNull(exception, nameof(exception));
            Ensure.String.IsNotNullOrWhiteSpace(key, nameof(key));
            Ensure.Any.IsNotNull(data, nameof(data));

            if (HasSerializedData(exception, key))
            {
                return exception;
            }

            var convertedData = ConvertData(data);

            if (convertedData != null)
            {
                // The conversion may have found that there was nothing of value to report
                exception.Data.Add(key, convertedData);
            }

            return exception;
        }

        /// <summary>
        /// Gets whether the exception contains data stored for the specified key.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="key">The key used to store the data.</param>
        /// <returns><c>true</c> if the exception contains data for the key; otherwise <c>false</c>.</returns>
        public static bool HasSerializedData(this Exception exception, string key)
        {
            Debug.Assert(exception != null, "No exception was provided");

            return exception.Data.Contains(key);
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

        private static object ConvertData(object data)
        {
            Debug.Assert(data != null, "No data provided");

            if (data.GetType().GetTypeInfo().IsValueType
                && data.GetType().Namespace == "System")
            {
                return data;
            }

            if (data is string dataAsString)
            {
                if (string.IsNullOrWhiteSpace(dataAsString))
                {
                    return null;
                }

                return dataAsString;
            }
            
            try
            {
                var serializedData = JsonConvert.SerializeObject(data, SerializerSettings);

                if (serializedData == "{}")
                {
                    return null;
                }

                return serializedData;
            }
            catch (Exception)
            {
                return data.ToString();
            }
        }

        /// <summary>
        /// Gets the default serializer settings used to append context data to exceptions.
        /// </summary>
        public static JsonSerializerSettings DefaultSerializerSettings => BuildSerializerSettings();

        /// <summary>
        /// Gets or sets the serializer settings used to append context data to exceptions.
        /// </summary>
        public static JsonSerializerSettings SerializerSettings { get; set; } = DefaultSerializerSettings;
    }
}