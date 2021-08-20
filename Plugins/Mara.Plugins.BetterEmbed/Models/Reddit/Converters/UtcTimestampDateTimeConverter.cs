using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mara.Plugins.BetterEmbeds.Models.Reddit.Converters
{
    public sealed class UtcTimestampDateTimeConverter : JsonConverter<DateTime>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(DateTime) ||
                   typeToConvert == typeof(DateTime?);
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TryGetDouble(out double value))
            {
                return DateTimeOffset.FromUnixTimeSeconds((long)value).UtcDateTime;
            }
            
            return DateTimeOffset.UnixEpoch.UtcDateTime;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(new DateTimeOffset(value).ToUnixTimeSeconds());
        }
    }

    public sealed class UtcTimestampDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(DateTimeOffset) ||
                    typeToConvert == typeof(DateTimeOffset?);
        }

        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TryGetDouble(out double value))
            {
                return DateTimeOffset.FromUnixTimeSeconds((long)value).UtcDateTime;
            }

            return DateTimeOffset.UnixEpoch.UtcDateTime;
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.ToUnixTimeSeconds());
        }
    }
}
