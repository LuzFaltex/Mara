using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mara.Plugins.BetterEmbeds.Models.Reddit.Converters
{
    public sealed class UtcTimestampConverter : JsonConverter<DateTime>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(DateTime) ||
                   typeToConvert == typeof(DateTime?) ||
                   typeToConvert == typeof(DateTimeOffset) ||
                   typeToConvert == typeof(DateTimeOffset?);
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64()).UtcDateTime;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(new DateTimeOffset(value).ToUnixTimeSeconds());
        }
    }
}
