using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mara.Common.Events;
using Mara.Common.Extensions;
using Microsoft.Extensions.Options;
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
        private readonly ILogger<BetterEmbedHandlerBase> _logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        protected BetterEmbedHandlerBase(ILogger<BetterEmbedHandlerBase> logger, Regex urlRegex, IDiscordRestChannelAPI channelApi, IOptions<JsonSerializerOptions> jsonSerializerOptions) : base(logger)
        {
            _logger = logger;
            _urlRegex = urlRegex;
            _channelApi = channelApi;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public override async Task<Result> HandleAsync(IMessageCreate gatewayEvent, CancellationToken cancellationToken = default)
        {
            // We don't care about system messages or calls
            if (gatewayEvent.Type != MessageType.Default)
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
                if (!string.IsNullOrEmpty(match.Groups["Prelink"].Value) ||
                    !string.IsNullOrEmpty(match.Groups["Postlink"].Value))
                {
                    hasPrePostText = true;
                }
            }

            if (!embeds.Any())
            {
                return Result.FromSuccess();
            }

            _logger.LogTrace(JsonSerializer.Serialize(embeds[0], _jsonSerializerOptions));

            // Post the embed(s)
            var postEmbed = await _channelApi.CreateMessageAsync(
                gatewayEvent.ChannelID,
                embeds: embeds,
                ct: cancellationToken
            );

            if (!postEmbed.IsSuccess)
            {
                return Result.FromError(postEmbed);
            }

            // If the post is only comprised of links, delete the invoking method.
            if (!hasPrePostText)
            {
                await _channelApi.DeleteMessageAsync(gatewayEvent.ChannelID, gatewayEvent.ID, "Deleting invocation method.", cancellationToken);
            }

            return Result.FromSuccess();
        }

        public abstract ValueTask<Result<IEmbed>> BuildEmbedAsync(Match match, IMessage message, CancellationToken cancellationToken = default);
    }
}
