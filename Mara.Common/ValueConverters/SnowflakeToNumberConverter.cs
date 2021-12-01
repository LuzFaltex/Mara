using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using DiscordConstants = Remora.Discord.API.Constants;
using Remora.Rest.Core;

namespace Mara.Common.ValueConverters
{
    /// <summary>
    /// Converts a Snowflake unto a ulong and back.
    /// </summary>
    public sealed class SnowflakeToNumberConverter : ValueConverter<Snowflake, ulong>
    {
        private static readonly ConverterMappingHints _defaultHints = new(precision: 20, scale: 0);

        /// <summary>
        /// Creates a new instance of the <see cref="SnowflakeToNumberConverter"/> type.
        /// </summary>
        public SnowflakeToNumberConverter() : base(sf => sf.Value, value => new(value, DiscordConstants.DiscordEpoch), _defaultHints)
        {
        }
    }
}
