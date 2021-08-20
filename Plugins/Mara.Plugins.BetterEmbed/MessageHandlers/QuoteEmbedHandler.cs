using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mara.Common.Results;
using Mara.Plugins.BetterEmbeds.Results;
using Mara.Plugins.BetterEmbeds.Services;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;
using Remora.Results;

namespace Mara.Plugins.BetterEmbeds.MessageHandlers
{
    public sealed class QuoteEmbedHandler : BetterEmbedHandlerBase
    {
        private static readonly Regex UrlRegex = new Regex(
            @"(?<Prelink>\S+\s+\S*)?(?<OpenBrace><)?https?://(?:(?:ptb|canary)\.)?discord(app)?\.com/channels/(?<GuildId>\d+)/(?<ChannelId>\d+)/(?<MessageId>\d+)/?(?<CloseBrace>>)?(?<Postlink>\S*\s+\S+)?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private readonly IQuoteService _quoteService;
        private readonly IDiscordRestChannelAPI _channelApi;

        public QuoteEmbedHandler(ILogger<QuoteEmbedHandler> logger, IQuoteService quoteService, IDiscordRestChannelAPI channelApi, IOptions<JsonSerializerOptions> jsonOptions) : base(logger, UrlRegex, channelApi, jsonOptions)
        {
            _quoteService = quoteService;
            _channelApi = channelApi;
        }

        public override async ValueTask<Result<IEmbed>> BuildEmbedAsync(Match match, IMessage message, CancellationToken cancellationToken = default)
        {
            if (!ulong.TryParse(match.Groups["GuildId"].Value, out _))
            {
                return new RegexError(message.Content, "Guild Id not found.");
            }

            if (!ulong.TryParse(match.Groups["ChannelId"].Value, out var channelId))
            {
                return new RegexError(message.Content, "Channel Id not found.");
            }

            if (!ulong.TryParse(match.Groups["MessageId"].Value, out var messageId))
            {
                return new RegexError(message.Content, "Message Id not found.");
            }

            if (!message.GuildID.HasValue)
                return new GuildRequiredError();

            // Get the channel the quoted message comes from
            var sourceChannelSnowflake = new Snowflake(channelId);
            var channelResult = await _channelApi.GetChannelAsync(sourceChannelSnowflake, cancellationToken);

            // Get the destination channel
            var destChannelResult = sourceChannelSnowflake.Equals(message.ChannelID)
                ? channelResult
                : await _channelApi.GetChannelAsync(message.ChannelID, cancellationToken); // Avoids second API call if they're the same channel.

            if (!channelResult.IsSuccess || channelResult.Entity is null)
                return Result<IEmbed>.FromError(channelResult);
            if (!destChannelResult.IsSuccess || destChannelResult.Entity is null)
                return Result<IEmbed>.FromError(channelResult);

            var sourceChannel = channelResult.Entity;
            var destChannel = destChannelResult.Entity;

            if (sourceChannel.Type != ChannelType.GuildText)
                return new BetterEmbedError(message, "Quoted message must be from a guild text channel.");
            if (destChannel.Type != ChannelType.GuildText)
                return new BetterEmbedError(message, "Reply must be in a guild text channel.");

            // If the source channel is nsfw, the destination channel must be too.
            if (sourceChannel.IsNsfw.HasValue && sourceChannel.IsNsfw.Value && !sourceChannel.IsNsfw.Equals(destChannel.IsNsfw))
            {
                return new BetterEmbedError(message,
                    "Quoted message comes from a channel marked Not Safe For Work. Destination channel must also be marked Not Safe For Work.");
            }

            // Try to get the message
            var quotedMessage =
                await _channelApi.GetChannelMessageAsync(sourceChannelSnowflake, new Snowflake(channelId), cancellationToken);

            if (!quotedMessage.IsSuccess)
                return Result<IEmbed>.FromError(quotedMessage.Error);

            var embed = await _quoteService.BuildQuoteEmbedAsync(quotedMessage.Entity, message.Author);

            return embed.IsSuccess
                ? embed
                : Result<IEmbed>.FromError(embed.Error);
        }
    }
}
