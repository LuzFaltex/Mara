using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mara.Common.Events;
using Mara.Common.Extensions;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Results;

namespace Mara.Plugins.BetterEmbeds
{
    public abstract class BetterEmbedHandlerBase : LoggingEventResponderBase<IMessageCreate>
    {
        private readonly Regex _urlRegex;
        private readonly IDiscordRestChannelAPI _channelApi;
        protected BetterEmbedHandlerBase(ILogger<BetterEmbedHandlerBase> logger, Regex urlRegex, IDiscordRestChannelAPI channelApi) : base(logger)
        {
            _urlRegex = urlRegex;
            _channelApi = channelApi;
        }

        public override async Task<Result> HandleAsync(IMessageCreate gatewayEvent, CancellationToken cancellationToken = default)
        {
            // We don't care about system messages or calls
            if (gatewayEvent.Type != MessageType.Default)
            {
                return Result.FromSuccess();
            }

            if (gatewayEvent.IsUserMessage())
            {
                return Result.FromSuccess();
            }

            if (!gatewayEvent.IsUserMessage())
            {
                return Result.FromSuccess();
            }

            var matches = _urlRegex.Matches(gatewayEvent.Content);

            var embeds = new List<IEmbed>();
            bool hasPrePostText = false;

            foreach (Match match in matches)
            {
                // If the link is surrounded with <>, skip. The user specifically wanted no embed.
                if (match.Groups["OpenBrace"].Success && match.Groups["CloseBrace"].Success)
                {
                    continue;
                }

                // Build the embed
                var buildEmbed = await BuildEmbedAsync(match, gatewayEvent, cancellationToken);

                if (!buildEmbed.IsSuccess)
                {
                    return Result.FromError(buildEmbed);
                }

                embeds.Add(buildEmbed.Entity);

                // If there is any text on either side of the URLs, record that.
                if (string.IsNullOrEmpty(match.Groups["Prelink"].Value) ||
                    string.IsNullOrEmpty(match.Groups["Postlink"].Value))
                {
                    hasPrePostText = true;
                }
            }

            if (!embeds.Any())
            {
                return Result.FromSuccess();
            }

            // Post the embed(s)
            var postEmbed = await _channelApi.CreateMessageAsync(
                gatewayEvent.ChannelID,
                embeds: embeds.AsReadOnly(),
                ct: cancellationToken
            );

            if (!postEmbed.IsSuccess)
            {
                return Result.FromError(postEmbed);
            }

            // If the post is only comprised of links, delete the invoking method.
            if (!hasPrePostText)
            {
                await _channelApi.DeleteMessageAsync(gatewayEvent.ChannelID, gatewayEvent.ID, cancellationToken);
            }

            return Result.FromSuccess();
        }

        public abstract ValueTask<Result<IEmbed>> BuildEmbedAsync(Match match, IMessage message, CancellationToken cancellationToken = default);
    }
}
