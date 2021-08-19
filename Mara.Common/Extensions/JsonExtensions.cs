using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Mara.Common.Extensions
{
    public static class JsonExtensions
    {
        public static TResult? ToObject<TResult>(this JsonElement element, JsonSerializerOptions jsonSerializerOptions)
            => ToObject<TResult>(element.GetRawText(), jsonSerializerOptions);
        public static TResult? ToObject<TResult, TClass>(this JsonElement element, ILogger<TClass> logger, JsonSerializerOptions jsonSerializerOptions)
        {
            var json = element.GetRawText();
            logger.LogTrace(json);
            return ToObject<TResult>(json, jsonSerializerOptions);
        }

        public static TResult? ToObject<TResult>(this JsonDocument document, JsonSerializerOptions jsonSerializerOptions)
        => ToObject<TResult>(document.RootElement.GetRawText(), jsonSerializerOptions);

        public static TResult? ToObject<TResult, TClass>(this JsonDocument document, ILogger<TClass> logger, JsonSerializerOptions jsonSerializerOptions)
        {
            var json = document.RootElement.GetRawText();
            logger.LogTrace(json);
            return ToObject<TResult>(json, jsonSerializerOptions);
        }

        private static TResult? ToObject<TResult>(string rawText, JsonSerializerOptions jsonSerializerOptions)
            => JsonSerializer.Deserialize<TResult>(rawText, jsonSerializerOptions);
    }
}
