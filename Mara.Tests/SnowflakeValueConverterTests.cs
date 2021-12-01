using Mara.Common.ValueConverters;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Remora.Rest.Core;
using Xunit;

using DiscordConstants = Remora.Discord.API.Constants;

namespace Mara.Tests
{
    public class SnowflakeValueConverterTests
    {
        private static readonly ValueConverter<Snowflake, ulong> _snowflakeToNumber = new SnowflakeToNumberConverter();

        private static readonly ulong foxtrekId = 197291773133979648;
        private static readonly ulong maraId = 801312393069199370;

        [Fact]
        public void CanConvertSnowflakesToNumbers()
        {
            var converter = _snowflakeToNumber.ConvertToProviderExpression.Compile();

            var foxtrek = new Snowflake(foxtrekId, DiscordConstants.DiscordEpoch);
            var mara = new Snowflake(maraId, DiscordConstants.DiscordEpoch);

            Assert.Equal(foxtrekId, converter(foxtrek));
            Assert.Equal(maraId, converter(mara));
        }

        [Fact]
        public void CanConvertSnowflakesToNumbersObject()
        {
            var converter = _snowflakeToNumber.ConvertToProvider;

            var foxtrek = new Snowflake(foxtrekId, DiscordConstants.DiscordEpoch);
            var mara = new Snowflake(maraId, DiscordConstants.DiscordEpoch);

            Assert.Equal(foxtrekId, converter(foxtrek));
            Assert.Equal(maraId, converter(mara));
            Assert.Null(converter(null));
        }

        [Fact]
        public void CanConvertNumbersToSnowflakes()
        {
            var converter = _snowflakeToNumber.ConvertFromProviderExpression.Compile();

            var foxtrek = new Snowflake(foxtrekId, DiscordConstants.DiscordEpoch);
            var mara = new Snowflake(maraId, DiscordConstants.DiscordEpoch);

            Assert.Equal(foxtrek, converter(foxtrekId));
            Assert.Equal(mara, converter(maraId));
        }

        [Fact]
        public void CanConvertNumbersToSnowflakesObject()
        {
            var converter = _snowflakeToNumber.ConvertFromProvider;

            var foxtrek = new Snowflake(foxtrekId, DiscordConstants.DiscordEpoch);
            var mara = new Snowflake(maraId, DiscordConstants.DiscordEpoch);

            Assert.Equal(foxtrek, converter(foxtrekId));
            Assert.Equal(mara, converter(maraId));
            Assert.Null(converter(null));
        }
    }
}
